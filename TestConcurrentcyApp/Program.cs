using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TestConcurrentcyApp.Model;

namespace TestConcurrentcyApp
{
    public class Person
    {
        public string Name { get; set; }
    }

    class Program
    {
        private static Student GetStudent(int id)
        {
            using (var db = new TestConcurrentcyDbContext())
            {
                return db.Students.Find(id);
            }
        }

        private static void UpdateStudent(Student student)
        {
            using (var db = new TestConcurrentcyDbContext())
            {
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        //模拟老项目的类库
        private static bool DebitOutputAccount(SqlConnection conn, SqlTransaction trans, int accountId, decimal amountToDebit)
        {
            int affectedRows = 0;
            var command = conn.CreateCommand();
            command.Transaction = trans;
            command.CommandType = CommandType.Text;
            command.CommandText = "Update OutputAccount set Balance=Balance-@amountToDebit where id=@accountId";
            command.Parameters.AddRange(new SqlParameter[]
            {
                new SqlParameter("@amountToDebit",amountToDebit),
                new SqlParameter("@accountId",accountId)
            });
            try
            {
                affectedRows = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return affectedRows > 0;
        }

        static void Main(string[] args)
        {
            #region ConcurrentcyTesting
            //Database.SetInitializer(new CreateDatabaseIfNotExists<TestConcurrentcyDbContext>());

            ////1.用户甲获取id=1的打赏者
            //var s1 = GetStudent(1);
            ////2.用户乙也获取id=1的打赏者
            //var s2 = GetStudent(1);
            ////3.用户甲只更新这个实体的Name字段
            //s1.LastName = "s1s1s1s1";
            //UpdateStudent(s1);
            ////4.用户乙只更新这个实体的Amount字段
            //s2.FirstName = "s2s2s2s2";
            //UpdateStudent(s2);

            // 测试EF并发 结论：
            // 1、通过增加字段 (a)字段名为RowVersion（规范命名）
            //                 (b) 数据类型为byte[]
            //                 (c) 数据标签[System.ComponentModel.DataAnnotations.Timestamp]
            // 然后 update-database 更新到数据库 生成对应列 其中sql数据类型为 timespamp
            // (d) 跳过(c) 可以通过 fluent api modelBuilder.Entity<Student>().Property(s => s.RowVersion).IsRowVersion();同样效果

            // 2、不增加字段方式 (a) 在现有字段上面打上标记 [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
            //                  (b) 跳过(a) 可以使用  fluent api modelBuilder.Entity<Student>().Property(x => x.FirstName).IsConcurrencyToken();同样效果

            // 通过以上配置才能激发EF对并发的感知也就是检测，否则默认不会检查任然会提交成功修改数据库以客户端为准，然后发生并发冲突的时候会触发DbUpdateConcurrencyException 异常，通过捕获异常，使用乐观并发锁方式，选择如下：（1）数据库优先  // Update the values of the entity that failed to save from the store ： ex.Entries.Single().Reload(); （2）客户端优先 // Update original values from the database 
            //      var entry = ex.Entries.Single();
            //      entry.OriginalValues.SetValues(entry.GetDatabaseValues());
            #endregion
               

            #region EF默认的事务处理

            //int outputId = 1, inputId = 1;
            //decimal transferAmount = 500m;
            //using (var db = new TestConcurrentcyDbContext())
            //{
            //    //1 检索事务中涉及的账户
            //    var outputAccount = db.OutputAccounts.Find(outputId);
            //    var inputAccount = db.InputAccounts.Find(inputId);
            //    //2 从输出账户上扣除1000
            //    outputAccount.Balance -= transferAmount;
            //    //3 从输入账户上增加1000
            //    inputAccount.Balance += transferAmount;

            //    //4 提交事务
            //    db.SaveChanges();
            //}
            #endregion


            #region 5.0  使用TransactionScope处理事务
            //int outputId = 1, inputId = 1;
            //decimal transferAmount = 1000m;
            //try
            //{
            //    using (var ts = new TransactionScope(TransactionScopeOption.Required))
            //    {
            //        var db1 = new TestConcurrentcyDbContext();
            //        var db2 = new TestConcurrentcyDbContext();
            //        //1 检索事务中涉及的账户
            //        var outputAccount = db1.OutputAccounts.Find(outputId);
            //        var inputAccount = db2.InputAccounts.Find(inputId);
            //        //2 从输出账户上扣除1000
            //        outputAccount.Balance -= transferAmount;
            //        //3 从输入账户上增加1000
            //        inputAccount.Balance += transferAmount;

            //        db1.SaveChanges();
            //        db2.SaveChanges();
            //        throw new System.ApplicationException("testing");
            //        ts.Complete();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message, e.InnerException?.Message);
            //}

            #endregion

            #region 6.0 使用EF6管理事务
            //int outputId = 1, inputId = 1;
            //decimal transferAmount = 1000m;
            //using (var db = new TestConcurrentcyDbContext())
            //{
            //    using (var trans = db.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            var sql = "Update OutputAccount set Balance=Balance-@amountToDebit where id=@outputId";
            //            db.Database.ExecuteSqlCommand(sql, new SqlParameter("@amountToDebit", transferAmount), new SqlParameter("@outputId", outputId));

            //            var inputAccount = db.InputAccounts.Find(inputId);
            //            inputAccount.Balance += transferAmount;
            //            db.SaveChanges();
            //            throw new System.ApplicationException("testing Rollback");
            //            trans.Commit();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //            trans.Rollback();
            //        }
            //    }
            //}
            #endregion

            #region 7.0 使用已存在的事务
            //int outputId = 1, inputId = 1;
            //decimal transferAmount = 1000m;
            //var connectionString = ConfigurationManager.ConnectionStrings["TestConcurrentcyConnectionString"].ConnectionString;
            //using (var conn = new SqlConnection(connectionString))
            //{
            //    conn.Open();
            //    using (var trans = conn.BeginTransaction())
            //    {
            //        try
            //        {
            //            var result = DebitOutputAccount(conn, trans, outputId, transferAmount);
            //            if (!result)
            //            {
            //                throw new ApplicationException("不能正常扣款！");
            //            }
            //            using (var db = new TestConcurrentcyDbContext(conn, contextOwnsConnection: false))
            //            {
            //                db.Database.UseTransaction(trans);
            //                var inputAccount = db.InputAccounts.Find(inputId);
            //                inputAccount.Balance += transferAmount;
            //                db.SaveChanges();
            //            }
            //            throw new ApplicationException("我是来测试 上下文数据事务一致性");
            //            trans.Commit();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //            trans.Rollback();
            //            Console.WriteLine("数据回滚完毕!");
            //        }
            //    }
            //}

            #endregion

            #region Await Async
            //FirsTask();
            #endregion

            #region Parallel
            //Parallel_For_Local_Test();
            #endregion

            #region 任务内联化(task inlining)----活用顶层任务工作线程
            //Task headTask = new Task(() =>
            //{
            //    DoSomeWork(null);
            //});
            //headTask.Start();
            //            分析：（目前内联机制只有出现在等待任务场景）
            //       这个示例，我们从Main方法主线程中创建了一个headTask顶层任务并开启。在headTask任务中又创建了三个嵌套任务并最后WaitAll() 这三个嵌套任务执行完成(嵌套任务安排在局部队列)。此时出现的情况就是headTask任务的线程被阻塞，而“任务内联化”技术会使用阻塞的headTask的线程去执行局部队列中的任务。因为减少了对额外线程需求，从而提升了程序性能。
            //         局部队列“通常”以LIFO的顺序抽取任务并执行，而不是像全局队列那样使用FIFO顺序。LIFO顺序通常用有利于数据局部性，能够在牺牲一些公平性的情况下提升性能。
            //数据局部性的意思是：运行最后一个到达的任务所需的数据都还在任何一个级别的CPU高速缓存中可用。由于数据在高速缓存中任然是“热的”，因此立即执行最后一个任务可能会获得性能提升。
            #endregion

            #region 反射性能测试

            //Person person = new Person();
            //PropertyInfo pInfo = typeof(Person).GetProperty("Name");
            //MethodInfo methodInfo = pInfo.GetSetMethod();

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //for (int i = 0; i < 10000000; i++)
            //{
            //    methodInfo.Invoke(person, new object[] { "djl" });
            //}
            //sw.Stop();
            //Console.WriteLine("反射方法执行：" + sw.ElapsedMilliseconds + "ms");
            //sw.Restart();

            //for (int i = 0; i < 10000000; i++)
            //{
            //    person.Name = "fuck";
            //}
            //sw.Stop();
            //Console.WriteLine("直接执行：" + sw.ElapsedMilliseconds + "ms");
            //sw.Restart();

            //// 1、必须知道 目标类型和修改目标属性(缺点)
            //Action<Person, string> MyAction = (Action<Person, string>)Delegate.CreateDelegate(typeof(Action<Person, string>), methodInfo);

            //for (int i = 0; i < 10000000; i++)
            //{
            //    MyAction(person, "fuck2");
            //}
            //sw.Stop();
            //Console.WriteLine("Delegate.CreateDelegate：" + sw.ElapsedMilliseconds + "ms");
            #endregion

            #region 冒泡复习
            //var list = new List<int>() { 2, 3, 4, 51, 2, 3, 4, 23, 5, 5, 3, 64, 77 };

            //for (int i = 0; i < list.Count; i++)
            //{
            //    for (int j = i + 1; j < list.Count; j++)
            //    {
            //        if (list[j] < list[i])
            //        {
            //            var temp = list[j];
            //            list[j] = list[i];
            //            list[i] = temp;
            //        }
            //    }
            //}
            //list.ForEach(x => Console.WriteLine(x));

            #endregion

            #region <C#>斐波那契数列(1、1、2、3、5、8、13、21、34 ...
            Console.WriteLine(Fn(5));
            #endregion

            // 测试
            Console.ReadKey();

        }

        private static int Fn(int num)
        {
            if (num <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (num == 1 || num == 2)
            {
                return 1;
            }
            return checked(Fn(num - 1) + Fn(num - 2)); // when n>46 memory will  overflow
        }

        /// <summary>
        /// 嵌套任务测试
        /// </summary>
        /// <param name="obj"></param>
        private static void DoSomeWork(object obj)
        {
            Console.WriteLine("任务headTask运行在线程“{0}”上",
                Thread.CurrentThread.ManagedThreadId);

            var taskTop = new Task(() =>
            {
                Thread.Sleep(500);
                Console.WriteLine("任务taskTop运行在线程“{0}”上",
                    Thread.CurrentThread.ManagedThreadId);
            });
            var taskCenter = new Task(() =>
            {
                Thread.Sleep(500);
                Console.WriteLine("任务taskCenter运行在线程“{0}”上",
                    Thread.CurrentThread.ManagedThreadId);
            });
            var taskBottom = new Task(() =>
            {
                Thread.Sleep(500);
                Console.WriteLine("任务taskBottom运行在线程“{0}”上",
                    Thread.CurrentThread.ManagedThreadId);
            });
            taskTop.Start();
            taskCenter.Start();
            taskBottom.Start();
            Task.WaitAll(new Task[] { taskTop, taskCenter, taskBottom });
        }

        /// <summary>
        /// 并行测试
        /// </summary>
        public static void Parallel_For_Local_Test()
        {
            int[] nums = Enumerable.Range(0, 1000000).ToArray<int>();
            long total = 0;
            ParallelLoopResult result = Parallel.For<long>(0, nums.Length,
                 () => { return 0; },
                 (j, loop, subtotal) =>
                 {
                     // 延长任务时间，更方便观察下面得出的结论
                     Thread.SpinWait(200);
                     Console.WriteLine("当前线程ID为：{0},j为{1}，subtotal为：{2}。"
                         , Thread.CurrentThread.ManagedThreadId, j.ToString(), subtotal.ToString());
                     if (j == 23)
                         loop.Break();
                     if (j > loop.LowestBreakIteration)
                     {
                         Thread.Sleep(4000);
                         Console.WriteLine("j为{0},等待4s种，用于判断已开启且大于阻断迭代是否会运行完。", j.ToString());
                     }
                     Console.WriteLine("j为{0},LowestBreakIteration为：{1}", j.ToString(), loop.LowestBreakIteration);
                     subtotal += nums[j];
                     return subtotal;
                 },
                 (finalResult) => Interlocked.Add(ref total, finalResult)
            );
            Console.WriteLine("total值为：{0}", total.ToString());
            if (result.IsCompleted)
                Console.WriteLine("循环执行完毕");
            else
                Console.WriteLine("{0}"
                    , result.LowestBreakIteration.HasValue ? "调用了Break()阻断循环." : "调用了Stop()终止循环.");
        }

        // 输出结果说明了前三个问题：

        // 我们可以看到输出1和6，2和5，3和4往往不是相同的线程，但是也未必不是相同的线程。 事实证明，await不会让当前线程等待await的结果，继而由等待线程继续向下执行。 而是，遇到await时，当前线程会被释放到线程池。在await返回结果后，在调用任意空闲线程向下执行。

        public static async Task<string> FirsTask()
        {
            Console.WriteLine("1.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
            var result = await SecondTask();
            Console.WriteLine("6.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
            return result;
        }

        public static async Task<string> SecondTask()
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("2.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
                var result = await client.GetAsync(
                    "http://www.google.com");
                await ThirdTask();
                Console.WriteLine("5.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
                return await result.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> ThirdTask()
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("3.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
                var result = await client.GetAsync(
                    "http://www.baidu.com");
                Console.WriteLine("4.当前线程是：" + Thread.CurrentThread.ManagedThreadId);
                return await result.Content.ReadAsStringAsync();
            }
        }

    }
}
