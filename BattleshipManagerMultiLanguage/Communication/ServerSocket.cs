using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BattleshipManagerMultiLanguage.Models;
using Newtonsoft.Json;

namespace BattleshipManagerMultiLanguage.Communication
{
    public class ServerSocket
    {
        #region Static

        #region - Private

        private static string debugShipsPositions;

        private static string playersJoined;

        #endregion

        #region - Public

        public static string data;
        public static ASCIIEncoding enc = new ASCIIEncoding( );
        public static Socket handler;
        public static List<Player> PlayerList;

        /// <summary>   Wait for players. </summary>
        /// <param name="playerCount">  Number of players.</param>
        /// <returns>   A List&lt;Player&gt; </returns>
        public static List<Player> WaitForPlayers( int playerCount )
        {
            Console.Clear( );
            byte[] bytes = new Byte[5024];

            IPHostEntry ipHostInfo = Dns.GetHostEntry( "localhost" );
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint( ipAddress, 8888 );

            Socket listener = new Socket( ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

            PlayerList = new List<Player>( );

            try
            {
                listener.Bind( localEndPoint );
                listener.Listen( 10 );

                Console.WriteLine( " >> Waiting for {0} player/s", playerCount - PlayerList.Count );

                while ( PlayerList.Count < playerCount )
                {
                    handler = listener.Accept( );
                    data = null;

                    int bytesRec = handler.Receive( bytes );
                    data += Encoding.ASCII.GetString( bytes, 0, bytesRec );

                    Player player;
                    string name;

                    if ( data.Contains( ';' ) )
                    {
                        var result = data.Split( ';' );

                        if ( PlayerList.Count( x => x.Token == result[0] ) > 0 )
                        {
                            handler.Shutdown( SocketShutdown.Both );
                            handler.Close( );
                            continue;
                        }

                        name = result[0];

                        player = new Player( result[0], result[1] == "true" || result[1] == "True" ? true : false );
                    }
                    else
                    {
                        name = data;
                        player = new Player( data, true );
                    }

                    PlayerList.Add( player );

                    if ( Program.DEBUG )
                    {
                        debugShipsPositions = "\n";

                        for ( int i = 0; i < 10; i++ )
                        {
                            for ( int j = 0; j < 10; j++ )
                            {
                                debugShipsPositions += player.ShipField[i, j] ? "#" : "~";
                            }

                            debugShipsPositions += "\n";
                        }
                    }

                    playersJoined += "\n" + String.Format( "  --> Player {0} joined the session", name );

                    data = JsonConvert.SerializeObject( player );

                    byte[] msg = Encoding.ASCII.GetBytes( data );

                    handler.Send( msg );
                    handler.Shutdown( SocketShutdown.Both );
                    handler.Close( );

                    Console.Clear( );
                    Console.WriteLine( String.Format( " >> Waiting for {0} player/s", playerCount - PlayerList.Count ) + playersJoined + debugShipsPositions );
                }

                listener.Close( );

                Console.WriteLine( "\n +>> Start Round\n" );

                return PlayerList;
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.ToString( ) );
            }

            return null;
        }

        /// <summary>   Wait for requests. </summary>
        /// <param name="players">  The players.</param>
        /// <returns>   A List&lt;Player&gt; </returns>
        public static List<Player> WaitForRequests( List<Player> players )
        {
            Program.printMap( players, false );

            byte[] bytes = new Byte[5024];

            IPHostEntry ipHostInfo = Dns.GetHostEntry( "localhost" );
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint( ipAddress, 8888 );

            Socket listener = new Socket( ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

            try
            {
                listener.Bind( localEndPoint );
                listener.Listen( 10 );

                while ( true )
                {
                    handler = listener.Accept( );
                    data = null;

                    int bytesRec = handler.Receive( bytes );
                    data += Encoding.ASCII.GetString( bytes, 0, bytesRec );

                    try
                    {
                        var result = data.Split( ';' );

                        var token = result[0];

                        var command = result[1];

                        var player = players.First( x => x.Token == token );

                        var enemyPlayer = players.First( x => x.Token != token );

                        if ( players[players.IndexOf( player )].winner )
                        {
                            Console.WriteLine( "\n{0} hat gewonnen\n", player.Name );
                            handler.Send( Encoding.ASCII.GetBytes( "WN" ) );
                            players[players.IndexOf( player )].gameEnded = true;
                            players[players.IndexOf( player )].winCount++;

                            if ( players.Count( x => x.gameEnded ) == players.Count )
                            {
                                if ( Program.RoundCount >= Program.MaxRoundCount )
                                    handler.Send( Encoding.ASCII.GetBytes( "END" ) );

                                Program.printMap( players, false );

                                break;
                            }

                            continue;
                        }

                        if ( players[players.IndexOf( player )].looser )
                        {
                            handler.Send( Encoding.ASCII.GetBytes( "LT" ) );
                            players[players.IndexOf( player )].gameEnded = true;

                            if ( players.Count( x => x.gameEnded ) == players.Count )
                            {
                                if ( Program.RoundCount >= Program.MaxRoundCount )
                                    handler.Send( Encoding.ASCII.GetBytes( "END" ) );

                                Program.printMap( players, false );

                                break;
                            }

                            continue;
                        }

                        if ( command == "SF" )
                        {
                            data = JsonConvert.SerializeObject( enemyPlayer.ShotField );

                            handler.Send( Encoding.ASCII.GetBytes( data ) );
                        }
                        else if ( command == "SA" )
                        {
                            if ( player.activePlayer )
                            {
                                var coords = result[2].Split( ',' );

                                enemyPlayer.ShotField[Convert.ToInt32( coords[0] ), Convert.ToInt32( coords[1] )] = true;

                                players[players.IndexOf( player )].activePlayer = false;
                                players[players.IndexOf( enemyPlayer )].activePlayer = true;

                                if ( enemyPlayer.ShipField[Convert.ToInt32( coords[0] ), Convert.ToInt32( coords[1] )] )
                                {
                                    Console.WriteLine( "{0} landete einen Treffer bei\t\t x: {1} | y: {2}", player.Name, coords[0], coords[1] );

                                    handler.Send( Encoding.ASCII.GetBytes( "T" ) );
                                    enemyPlayer.ShipField[Convert.ToInt32( coords[0] ), Convert.ToInt32( coords[1] )] = false;

                                    var shipPartCounter = 0;

                                    for ( int i = 0; i < 10; i++ )
                                    {
                                        for ( int j = 0; j < 10; j++ )
                                        {
                                            if ( enemyPlayer.ShipField[i, j] )
                                                shipPartCounter++;
                                        }
                                    }

                                    if ( shipPartCounter <= 0 )
                                    {
                                        players[players.IndexOf( player )].winner = true;
                                        players[players.IndexOf( enemyPlayer )].looser = true;
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine(string.Format("{0} schoss daneben", player.Name));
                                    handler.Send( Encoding.ASCII.GetBytes( "W" ) );
                                }
                            }
                            else
                            {
                                handler.Send( Encoding.ASCII.GetBytes( "NT" ) );
                            }
                        }
                        else if ( command == "ST" )
                        {
                            var data = JsonConvert.SerializeObject( player );
                            handler.Send( Encoding.ASCII.GetBytes( data ) );
                        }
                        else
                        {
                            handler.Send( Encoding.ASCII.GetBytes( "ER" ) );
                        }

                        data = JsonConvert.SerializeObject( player );

                        byte[] msg = Encoding.ASCII.GetBytes( data );

                        handler.Send( msg );
                    }
                    catch ( Exception e )
                    {
                        handler.Send( Encoding.ASCII.GetBytes( "ER" ) );
                    }
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.ToString( ) );
            }

            listener.Close( );

            return PlayerList;
        }

        #endregion

        #endregion
    }
}