// Copyright 2025 Michael Hoopmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.IPC.Pipes
{
  /// <summary>
  /// TaskQueue class has nothing to do with the Pipes library. It's just a way to schedule tasks
  /// with a static scheduler that can be used anywhere in the application.
  /// </summary>
  class TaskQueue
  {
    private readonly TaskScheduler tasks;
    private static TaskScheduler MyTasks
    {
      get
      {
        return (SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Default);
      }
    }

    public event TaskSuccessEventHandler Succeeded;
    public event TaskExceptionEventHandler Error;

    public TaskQueue() : this(MyTasks)
    {
    }

    public TaskQueue(TaskScheduler myTasks)
    {
      tasks = myTasks;
    }

    /// <summary>
    /// Places a task in the queue.
    /// </summary>
    /// <param name="action">The method to be run</param>
    public void DoTask(Action action)
    {
      new Task(GoTask, action, CancellationToken.None, TaskCreationOptions.LongRunning).Start();
    }

    /// <summary>
    /// Private wrapper around the method in the task. Runs the method and indicates success or failure.
    /// </summary>
    /// <param name="obj"></param>
    private void GoTask(object obj)
    {
      if (obj == null) return;
      var action = (Action)obj;
      try
      {
        action();
        Callback(Success);
      }
      catch (Exception e)
      {
        Callback(() => Fail(e));
      }
    }

    private void Success()
    {
      Succeeded?.Invoke();
    }

    private void Fail(Exception exception)
    {
      Error?.Invoke(exception);
    }

    private void Callback(Action action)
    {
      Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, tasks);
    }
  }

  internal delegate void TaskSuccessEventHandler();
  internal delegate void TaskExceptionEventHandler(Exception exception);

}
