using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

namespace DKK
{
	public class Async : MonoBehaviour
	{
		
		public delegate object MethodToDo (object arg);

		static public Async self;

		void Awake ()
		{
			self = this;
		}

		/// <summary>
		/// Operations to do. Do some operations in new thread.
		/// </summary>
		/// <param name="Method">Method.</param>
		/// <param name="arg">Argument.</param>
		/// <param name="Callback">Callback.</param>
		static public void OperationToDo (MethodToDo Method, object arg = null, ObjectMethod Callback = null, VoidMethod UpdateWhileRunning = null)
		{
			self.Operation (Method, arg, Callback, UpdateWhileRunning);
		}

		private void Operation (MethodToDo AsyncMethod, object arg, ObjectMethod Callback, VoidMethod UpdateWhileRunning)
		{
			AsyncResult asyncResult = new AsyncResult (Callback);

			List<object> args = new List<object> ();
			args.Add (AsyncMethod);
			args.Add (arg);
			args.Add (asyncResult);

			BackgroundWorker worker = new BackgroundWorker ();
			worker.DoWork += (o, e) => {
				List<object> genericlist = e.Argument as List<object>;
				MethodToDo MethodTD = genericlist [0] as MethodToDo;
				genericlist.Add (MethodTD (genericlist [1]));
				e.Result = genericlist;
			};
			worker.RunWorkerCompleted += (o, e) => {
				List<object> genericlist = e.Result as List<object>;
				AsyncResult asyncRes = genericlist [2] as AsyncResult;
				asyncRes.result = genericlist [3];
				asyncRes.isDone = true;
			};

			worker.RunWorkerAsync (args);
			StartCoroutine (Collector (asyncResult, UpdateWhileRunning));
		}

		IEnumerator Collector (AsyncResult asyncResult, VoidMethod UpdateWhileRunning)
		{
			while (!asyncResult.isDone) {
				if (UpdateWhileRunning != null)
					UpdateWhileRunning ();
				yield return null;
			}

			asyncResult.Return ();
		}
	}

	public class AsyncResult
	{
		public ObjectMethod Callback;
		public object result;
		public bool isDone = false;

		public AsyncResult (ObjectMethod Callback)
		{
			this.Callback = Callback;
		}

		public void Return ()
		{
			if (Callback != null)
				Callback (result);
		}
	}
}