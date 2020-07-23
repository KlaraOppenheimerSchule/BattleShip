using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace BattleshipBotCSharp
{
    class Program
    {
        #region Static

        #region - Public

        public static Player Player;
        public static List<List<int>> ShotField = new List<List<int>>( );

        public static int Main( String[] args )
        {
            var random = new Random( );

            SetShootField( );

            Player = JsonConvert.DeserializeObject<Player>( SendRequest( "CSharp" ) );

            while ( true )
            {
                var tmpList = ShotField[random.Next( ShotField.Count )];
                var response = SendRequest( Player.Token + ";SA;" + tmpList[0] + "," + tmpList[1] );

                if ( response == "W" || response == "T" )
                    ShotField.Remove( tmpList );

                if ( response == "WN" || response == "LT" || ShotField.Count == 0 )
                {
                    SetShootField( );
                    Thread.Sleep( 1000 );
                }

                if ( response == "END" )
                    break;

                Thread.Sleep( 5 );
            }

            return 0;
        }

        /// <summary>   Sends a request. </summary>
        /// <param name="request">  The request.</param>
        /// <returns>   A string. </returns>
        public static string SendRequest( string request )
        {
            byte[] bytes = new byte[5024];

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry( "localhost" );
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint( ipAddress, 8888 );

                Socket sender = new Socket( ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

                try
                {
                    sender.Connect( remoteEP );

                    byte[] msg = Encoding.ASCII.GetBytes( request );

                    int bytesSent = sender.Send( msg );

                    int bytesRec = sender.Receive( bytes );
                    var response = Encoding.ASCII.GetString( bytes, 0, bytesRec );

                    sender.Close( );

                    return response;
                }
                catch ( Exception e )
                {
                }
            }
            catch ( Exception e )
            {
            }

            return string.Empty;
        }

        /// <summary>   Sets shoot field. </summary>
        public static void SetShootField( )
        {
            for ( int i = 0; i < 10; i++ )
            {
                for ( int j = 0; j < 10; j++ )
                {
                    ShotField.Add( new List<int> { i, j } );
                }
            }
        }

        #endregion

        #endregion
    }
}