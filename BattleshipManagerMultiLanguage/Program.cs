using System;
using System.Collections.Generic;
using BattleshipManagerMultiLanguage.Communication;
using BattleshipManagerMultiLanguage.Models;

namespace BattleshipManagerMultiLanguage
{
    public class Program
    {
        #region Static

        #region - Private

        private static List<Player> PlayerList = new List<Player>( );

        static void Main( string[] args )
        {
            while ( true )
            {
                if ( RoundCount == 0 )
                {
                    PlayerList = ServerSocket.WaitForPlayers( 2 );
                    var rand = new Random( );
                    PlayerList[rand.Next( PlayerList.Count - 1 )].activePlayer = true;
                }
                else
                {
                    foreach ( var player in PlayerList )
                    {
                        player.Ships = player.SetShips( player.createShips( ) );
                    }
                }

                RoundCount++;

                PlayerList = ServerSocket.WaitForRequests( PlayerList );

                foreach ( var player in PlayerList )
                {
                    PlayerList[PlayerList.IndexOf( player )].gameEnded = false;
                    PlayerList[PlayerList.IndexOf( player )].winner = false;
                    PlayerList[PlayerList.IndexOf( player )].looser = false;
                    PlayerList[PlayerList.IndexOf( player )].ShotField = new bool[10, 10];
                    PlayerList[PlayerList.IndexOf( player )].ShipField = new bool[10, 10];
                }

                if ( ServerSocket.handler.IsBound )
                    ServerSocket.handler.Close( );

                if ( RoundCount >= MaxRoundCount )
                    break;
            }

            //Console.Clear( );

            foreach ( var player in PlayerList )
            {
                Console.WriteLine( "{0} hat {1} ({2}%) von {3} Runden gewonnen", player.Name, player.winCount, Decimal.Round( Convert.ToDecimal( player.winCount ) / Convert.ToDecimal( MaxRoundCount ), 2 ), MaxRoundCount );
            }

            Console.ReadLine( );
            Console.ReadLine( );
        }

        #endregion

        #region - Public

        public static bool DEBUG = false;
        public static int MaxRoundCount = 5;

        public static int RoundCount;

        /// <summary>   Print map. </summary>
        /// <param name="players">      The players.</param>
        /// <param name="clearConsole"> True to clear console.</param>
        public static void printMap( List<Player> players, bool clearConsole )
        {
            if ( clearConsole )
                Console.Clear( );

            var shipsPositions = string.Empty;

            foreach ( var player in players )
            {
                shipsPositions += player.Name + "\n";

                for ( int i = 0; i < 10; i++ )
                {
                    for ( int j = 0; j < 10; j++ )
                    {
                        shipsPositions += player.ShotField[i, j] ? "X" : player.ShipField[i, j] ? "#" : "~";
                    }

                    shipsPositions += "\n";
                }

                shipsPositions += "\n\n";
            }

            Console.WriteLine( shipsPositions );
        }

        #endregion

        #endregion
    }
}