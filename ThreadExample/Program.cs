using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadExample
{
  class ExampleJob
  {
    static AutoResetEvent autoResetEvent = new AutoResetEvent( false );
    public Dictionary<string, bool> _flags = new Dictionary<string, bool>();

    private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
    private readonly object _lockini = new object();

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
    public void Job( object test )
    {
      for( int i = 1; i <= 10; i++ )
      {
        //  Console.WriteLine( $"{test.ToString()} -> {i}" );
        WriteToFileThreadSafe( $"{test.ToString()} -> {i}",  "test.txt" );

        if( i == 5 )
        {
          lock( _lockini )
          {
            _flags[test.ToString()] = true;
          } 
          autoResetEvent.WaitOne();
        }
      }
    }

    public void Satpam()
    {
      Console.WriteLine( FlagIsUp() );
      while ( FlagIsUp() )
      {
        // Console.WriteLine( FlagIsUp() );
        foreach( KeyValuePair<string, bool> each in _flags )
        {
          Console.Write( each.Key + ":" + each.Value + " " );
        }
        Console.WriteLine();
        Thread.Sleep(100);
      }
      Console.WriteLine( FlagIsUp() );
      Console.WriteLine("udahan");

      autoResetEvent.Set();
    }

    public bool FlagIsUp()
    {
      bool ada = false;
      bool semua = true;

      lock (_lockini)
      {
        foreach( KeyValuePair<string, bool> each in _flags )
        {
          if( each.Value )
            ada = true;

          semua = semua && each.Value;
        }
      }

      if( semua ) { return false; }
        
      if( ada ) { return true; }

      return false;
    }
  }
  class Program
  {
    static void Main( string[] args )
    {
      ExampleJob ex = new ExampleJob();

      for( int i = 1; i <= 5; i++ )
      {
        ex._flags.Add( "th" + i.ToString(), false );
      }

      for (int i = 1; i <= 5; i++ )
      {
        new Thread( ex.Job ).Start( "th" + i.ToString() );
      }

      Thread.Sleep( 1000 );
      new Thread( ex.Satpam ).Start();
    }
  }
}
