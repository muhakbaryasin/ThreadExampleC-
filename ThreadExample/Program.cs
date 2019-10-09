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
    private int _threadNumber;

    public ExampleJob (int threadNumber)
    {
      _threadNumber = threadNumber;

      for( int i = 0; i < _threadNumber; i++ )
      {
        var job = new Thread( Job );
        _resetEvents.Add( "th" + i.ToString(), new AutoResetEvent( false ) );
        
        if( i != 0)
          _resetEvents["th" + i.ToString()].WaitOne();

        job.Start( i );
      }
    }

    public void Job( object ID )
    {
      int intID = Convert.ToInt32( ID.ToString() );

      // doing simple iteration
      for( int i = 1; i <= 10; i++ )
      {
        WriteToFileThreadSafe( $"th{(intID + 1).ToString()} -> {i}", "test.txt" );

        // when the count is 5, then current thread need to wait while the next thread can be proceed
        if( i == 5 )
          toogleWait(intID);
      }

      // enqueue thread to finish
      toogleWait( intID, true );
    }

    private void toogleWait(int ID, bool release= false)
    {
      // set next thread to continue
      _resetEvents["th" + ( (ID + 1) % _threadNumber ).ToString()].Set();

      // set current thread to wait
      // but when release is true, we let the thread finish
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
