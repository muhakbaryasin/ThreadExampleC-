using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace ThreadExample
{
  class Program
  {
    static void Main( string[] args )
    {
      ExampleJob ex = new ExampleJob(5);
    }
  }

class ExampleJob
  {
    private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
    private Dictionary<string, AutoResetEvent> _resetEvents = new Dictionary<string, AutoResetEvent>();
    private int _jumlahThread;

    public ExampleJob (int jumlahThread)
    {
      _jumlahThread = jumlahThread;

      for( int i = 1; i <= _jumlahThread; i++ )
      {
        var job = new Thread( Job );
        _resetEvents.Add( "th" + i.ToString(), new AutoResetEvent( false ) );
        
        if( i != 1)
          _resetEvents["th" + i.ToString()].WaitOne();
        job.Start( i );
      } 

      // Thread.Sleep( 1000 );
    }

    public void Job( object ID )
    {
      // _resetEvents["th" + ID.ToString()].Set();
      int intID = Convert.ToInt32( ID.ToString() );

      for( int i = 1; i <= 10; i++ )
      {
        //  Console.WriteLine( $"{test.ToString()} -> {i}" );
        WriteToFileThreadSafe( $"th{ID.ToString()} -> {i}", "test.txt" );

        if( i == 5 )
        {
          toogleWait(intID);
        }
      }

      toogleWait( intID, true );
    }

    private void toogleWait(int ID, bool release= false)
    {
     
        if( ID < _jumlahThread )
          _resetEvents["th" + ( ID + 1 ).ToString()].Set();
        else if( ID == _jumlahThread)
          _resetEvents["th1"].Set();
      
        if (!release)
          _resetEvents["th" + ID.ToString()].WaitOne();

    }

    public void WriteToFileThreadSafe( string text, string path )
    {
      // Set Status to Locked
      _readWriteLock.EnterWriteLock();
      try
      {
        // Append text to the file
        using( StreamWriter sw = File.AppendText( path ) )
        {
          sw.WriteLine( text );
          sw.Close();
        }
      }
      finally
      {
        // Release lock
        _readWriteLock.ExitWriteLock();
      }
    }
    
  }
}
