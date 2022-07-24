using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadsDemo
{
    public class MainThread
    {
        private ILogger _Logger;
        private BlockingCollection<long> _Queue = new BlockingCollection<long>();
        private int _MinThreadCount;
        private int _MaxThreadCount;
        private List<long> _Result; 
    
        public MainThread()
        {
            _Logger = NLog.LogManager.GetCurrentClassLogger();
            // set to processor count
            _MinThreadCount = System.Environment.ProcessorCount;
            _MaxThreadCount = 20;
        }

        public void Start()
        {
            var maxThreadCount = System.Environment.ProcessorCount;
            ThreadPool.SetMinThreads(_MinThreadCount, _MinThreadCount);// 第一个参数是 worker 线程数，第二个是异步IO线程数
            Task.Factory.StartNew(PopulateQueue);
            Task.Factory.StartNew(ConsumeQueue, TaskCreationOptions.LongRunning);
        }

        // populate the queue
        private void PopulateQueue()
        {
            for (int i = 1; i < 1000; i++)
            {
                _Queue.Add(i);
                if (i / 100 == 0)
                {
                    Thread.Sleep(100);
                }
            }
            _Queue.CompleteAdding();
        }

        // consume the queue
        private void ConsumeQueue()
        {
            var partitioner = Partitioner.Create(_Queue.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _MaxThreadCount };
            _Logger.Info("Starting");
            _Result = new List<long>();
            while (!_Queue.IsCompleted)
            {
                Parallel.ForEach(partitioner, parallelOptions, Calculate);
            }
            _Logger.Info("Finished");

            _Logger.Info("Total:" + _Result.Count);

            _Result.ForEach(number => {
                _Logger.Info(number);
            });
        }

        // 27的倍数
        private void Calculate(long number)
        {
            _Logger.Info("Processing:" + number);
            if (number % 27 == 0)
            {
                _Logger.Info("=====Got:" + number);
                _Result.Add(number);
            }
        }
    }
}
