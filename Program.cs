using System;
using System.Diagnostics;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        CancellationTokenSource tokensource = new CancellationTokenSource();
        CancellationToken ct = tokensource.Token;     //cancels the task if it has not yet started

        Console.WriteLine("Hello, World!");
        Console.WriteLine("Main Method Started......");

        Task.Run(() => TaskMethod(), ct);               //Start a task using the Run() function
        
        var T = ExceptionAsync();                   //Assigning a task
        try
        {
            var result = await T;                   //Exceptions are handled at this line only
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception occered : " + ex.Message);
        }
        
        SomeMethod(ct);

        var OneTask = Task.Run(() => Task1()); 
        var TwoTask = Task.Run(() => Task2());
        var ThreeTask = Task.Run(() => Task3());

        Stopwatch sw = new Stopwatch();
        sw.Start();
        int[] finishedTasks = await Task.WhenAll(OneTask, TwoTask, ThreeTask);  //returns only when all the tasks are completed
        Console.WriteLine(sw.ElapsedMilliseconds + "  Time taken -  WhenAll is used");
        
        
        
        var Task4 = Task1();
        var Task5 = Task2();
        var Task6 = Task3();

        sw.Restart();
        var finishedTask = await Task.WhenAny(Task4, Task5, Task6);     //return when any one of the task is completed
        Console.WriteLine(sw.ElapsedMilliseconds + "  Time taken - WhenAny is used");
        sw.Stop();

        Console.WriteLine("Main Method End");
        Thread.Sleep(5000);
        Console.WriteLine("4 SECONDS ELAPSED after end of Main Method");
       
        CancellableMethod();
        DoWork(new CancellationToken());
        Console.ReadKey();
    }
    public async static void SomeMethod(CancellationToken ct)
    {
        Console.WriteLine("Some Method Started......");
        //Thread.Sleep(TimeSpan.FromSeconds(10));       //this will make the whole thread sleep, and not run parallely
        await Task.Delay(TimeSpan.FromSeconds(5));     //frees the thread for 10 seconds, after which it will continue execution of this function
        Console.WriteLine("\n");
        Console.WriteLine("Some Method Ends with Another Method value - loading... wait 15 secs");
        int num = await AnotherMethod(ct);            //async waits until AnotherMethos returns a value
        Console.Write(" " + num + " = Value " );
    }

    public static async Task<int> AnotherMethod(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
        return 42;
    }

    public static Task TaskMethod()
    {
        Console.WriteLine("This is a task");
        return Task.CompletedTask;
    }

    private static async Task<string> ExceptionAsync()
    {
        throw new Exception("Throwing an exception !!! ");
    }

    static async Task<int> Task1()      //takes 5 secs to return
    {
        await Task.Delay(5000);
        return 11;
    }

    static async Task<int> Task2()      //takes 3 secs to return
    {
        await Task.Delay(3000);
        return 22;
    }
    static async Task<int> Task3()      //takes 6 seconds to return
    {
        await Task.Delay(6000);
        return 33;
    }

    public static async Task CancellableMethod()
    {
        var tokenSource = new CancellationTokenSource();
                                                                    // Queue long running tasks
        for (int i = 0; i < 15; ++i)
        {
            Task.Run(() => DoSomeWork(tokenSource.Token), tokenSource.Token);
        }
        
        tokenSource.Cancel();                               // After some delay/when you want manual cancellation
    }
    // Runs on a different thread
    public static async Task DoSomeWork(CancellationToken ct)
    {
        int maxIterations = 100000;
        for (int i = 0; i < maxIterations; ++i)
        {
                                                // long running work
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Task cancelled.");
                ct.ThrowIfCancellationRequested();
            }
        }
    }

    public static async void DoWork(CancellationToken ct)
    {
        var internalTokenSource = new CancellationTokenSource();
        internalTokenSource.CancelAfter(10000);                         //internally cancels after 10k ms
        var internalToken = internalTokenSource.Token;
        var externalToken = ct;
        using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalToken, externalToken))       
            //linking cancelation tokens thereby can cancel due to various sources/reasons
        {
            try
            {
                await DoWorkInternal(linkedCts.Token);
            }
            catch (Exception ex)
            {
                if (internalToken.IsCancellationRequested)
                {
                    Console.WriteLine("Operation timed out");
                }

                if (externalToken.IsCancellationRequested)
                {
                    Console.WriteLine("Cancelling per user request.");
                }
            }
        }
    }

    public static async Task DoWorkInternal(CancellationToken token)
    {
        while(true)
            token.ThrowIfCancellationRequested();
    }
}