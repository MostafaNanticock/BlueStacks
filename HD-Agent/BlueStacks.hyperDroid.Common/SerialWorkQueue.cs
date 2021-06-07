using System.Collections.Generic;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{
	public class SerialWorkQueue
	{
		public delegate void Work();

		private Thread mThread;

		private Queue<Work> mQueue;

		private object mLock;

		public SerialWorkQueue()
		{
			this.mQueue = new Queue<Work>();
			this.mLock = new object();
			this.mThread = new Thread(this.Run);
			this.mThread.IsBackground = true;
		}

		public void Start()
		{
			this.mThread.Start();
		}

		public void Join()
		{
			this.mThread.Join();
		}

		public void Stop()
		{
			this.Enqueue(null);
		}

		public void Enqueue(Work work)
		{
			lock (this.mLock)
			{
				this.mQueue.Enqueue(work);
				Monitor.PulseAll(this.mLock);
			}
		}

		private void Run()
		{
			while (true)
			{
				Work work = default(Work);
				lock (this.mLock)
				{
					while (this.mQueue.Count == 0)
					{
						Monitor.Wait(this.mLock);
					}
					work = this.mQueue.Dequeue();
				}
				if (work != null)
				{
					work();
					continue;
				}
				break;
			}
		}
	}
}
