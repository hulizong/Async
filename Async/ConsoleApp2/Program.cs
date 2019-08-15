using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {

            Dont();
            Console.WriteLine("结束");
            Console.ReadLine();
        }

        static async Task ThrowAfterAsync(int ms, string msg)
        {
            await Task.Delay(ms);
            throw new Exception(msg);
        }

        private static async void Dont()
        {
            Task taskResult = null;
            try
            {
                Task t1 = ThrowAfterAsync(200, "第一个错误");
                Task t2 = ThrowAfterAsync(100, "第二个错误");
                await (taskResult=Task.WhenAll(t1,t2)); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                foreach (var item in taskResult.Exception.InnerExceptions)
                {
                    Console.WriteLine(item.Message);
                }
            }
        }

        /// <summary>
        /// 转换异步模式
        /// </summary>
        private static async void ConvertingAsync()
        {
            HttpWebRequest request = WebRequest.Create("http://www.cninnovation.com/") as HttpWebRequest;
            using (WebResponse response = await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse(null, null), request.EndGetResponse))
            {
                Stream stream = response.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    Console.WriteLine(content.Substring(0, 100));
                }
            }
        }



        /// <summary>
        /// ValueTask
        /// </summary>
        private readonly static Dictionary<string, string> names = new Dictionary<string, string>();

        private static async ValueTask<string> GetStringDicAsync(string name)
        {
            if (names.TryGetValue(name, out string result))
            {
                return result;
            }
            else
            {
                result = await GetStringAsync(name);
                names.Add(name, result);
                return result;
            }
        }

        /// <summary>
        /// WhenAll并行运行异步方法
        /// </summary>
        private static async void ManyAsyncFunWithWhenAll()
        {
            Task<string> result1 = GetStringAsync("张三");
            Task<string> result2 = GetStringAsync("李四");
            await Task.WhenAll(result1, result2);
            Console.WriteLine($"第一个人是{result1.Result},第二个人是{result2.Result}");
        }



        /// <summary>
        /// 调用多个异步方法
        /// </summary>
        private static async void ManyAsyncFun()
        {
            var result1 = await GetStringAsync("张三");
            var result2 = await GetStringAsync("李四");
            Console.WriteLine($"第一个人是{result1},第二个人是{result2}");
        }



        /// <summary>
        /// 使用ContinueWith延续任务
        /// </summary>
        /// <param name="name"></param>
        private static void GetStringContinueAsync(string name)
        {
            SeeThreadAndTask($"开始   运行{nameof(GetStringContinueAsync)}");
            var result = GetStringAsync(name);
            result.ContinueWith(t =>
            {
                string answr = t.Result;
                Console.WriteLine(answr);

                SeeThreadAndTask($"结束    运行{nameof(GetStringContinueAsync)}");
            });
        }



        /// <summary>
        /// 使用Awaiter另外方式
        /// </summary>
        /// <param name="name"></param>
        private static void GetSelfAwaiters(string name)
        {
            SeeThreadAndTask($"运行{nameof(GetSelfAwaiter)}");
            string awaiter = GetStringAsync(name).GetAwaiter().GetResult();

            Console.WriteLine(awaiter);
            SeeThreadAndTask($"运行{nameof(GetSelfAwaiter)}");

        }

        /// <summary>
        /// 使用Awaiter
        /// </summary>
        /// <param name="name"></param>
        private static void GetSelfAwaiter(string name)
        {
            SeeThreadAndTask($"运行{nameof(GetSelfAwaiter)}");
            TaskAwaiter<string> awaiter = GetStringAsync(name).GetAwaiter();
            awaiter.OnCompleted(OnCompletedAwauter);

            void OnCompletedAwauter()
            {
                Console.WriteLine(awaiter.GetResult());
                SeeThreadAndTask($"运行{nameof(GetSelfAwaiter)}");
            }
        }
        /// <summary>
        /// 调用异步方法
        /// </summary>
        /// <param name="name"></param>
        private static async void GetSelfAsync(string name)
        {
            SeeThreadAndTask($"开始运行{nameof(GetSelfAsync)}");
            string result = await GetStringAsync(name);
            Console.WriteLine(result);
            SeeThreadAndTask($"结束运行{nameof(GetSelfAsync)}");
        }


        /// <summary>
        /// Task.Run创建任务
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static Task<string> GetStringAsync(string name) =>
            Task.Run<string>(() =>
            {
                SeeThreadAndTask($"运行{nameof(GetStringAsync)}");
                return GetString(name);
            });


        /// <summary>
        /// 同步方法
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static string GetString(string name)
        {
            SeeThreadAndTask($"运行{nameof(GetString)}");
            Task.Delay(3000).Wait();
            return $"你好，{name}";
        }

        /// <summary>
        /// 观察线程变化情况
        /// </summary>
        /// <param name="info"></param>
        public static void SeeThreadAndTask(string info)
        {
            string taskinfo = Task.CurrentId == null ? "没任务" : "任务id是：" + Task.CurrentId;
            Console.WriteLine($"{info} 在线程{Thread.CurrentThread.ManagedThreadId}和{taskinfo}中执行");
        }
    }
}

