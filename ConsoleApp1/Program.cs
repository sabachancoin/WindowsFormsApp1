using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

        }

        static void test16()
        {
            const int count = 1000000;
            int[] a = new int[1000];
            bool dummy;

            for (int i = 0; i < a.Length; i++)
                a[i] = i;

            DateTime start1 = DateTime.Now;
            for (int i = 0; i < count; i++)
                dummy = (from n in a select n).Count() > 0;
            Console.WriteLine(DateTime.Now - start1);

            DateTime start2 = DateTime.Now;
            for (int i = 0; i < count; i++)
                dummy = (from n in a select n).Any();
            Console.WriteLine(DateTime.Now - start2);
        }

        static void test15()
        {
            int count = (from n in Enumerable.Range('A', 26)
                         where (n % 2) == 0
                         select n).Count();

            Console.WriteLine(count);
        }

        static void test14()
        {
            string s = new string((from n in Enumerable.Range('A', 26)
                                   where (n % 2) == 0
                                   select (char)n).ToArray());

            Console.WriteLine(s);
        }

        private static int count = 0;
        private static int conversion(int n)
        {
            count++;
            return n;
        }
        static void test13()
        {
            int[] a = { 1, 2, 3 };
            int[] b = { 4, 5, 6 };

            var q = from n in a
                    let cn = conversion(n)
                    from m in b
                    select cn + m;
            //var q = from n in a
            //        from m in b
            //        select conversion(n) + m;

            foreach (var n in q)
                Console.WriteLine(n);

            Console.WriteLine("conversion called: {0}", count);
        }

        class 家
        {
            public string 姓;
            public string[] 構成員;
        }
        static void test12()
        {
            家[] 家々 =
            {
                new 家() {
                    姓 = "磯崎",
                    構成員 = new [] { "波平", "フネ", "カツオ", "ワカメ"},
                },
                new 家() {
                    姓 = "フグ山",
                    構成員 = new [] { "サザエ", "マスオ", "タラオ"},
                },
            };

            foreach (var 人 in
                from その家 in 家々
                from 名 in その家.構成員
                select new { その家, 名 })
                Console.Write("{0}{1}" + Environment.NewLine, 人.その家.姓, 人.名);
        }

        static void dumpArray(string[] array)
        {
            foreach (var s in array.DefaultIfEmpty("リストはカラです"))
                Console.WriteLine(s);
        }
        static void test11()
        {
            string[] t1 = { "Hello!", "Linq!" };
            string[] t2 = { };
            dumpArray(t1);
            dumpArray(t2);
        }

        class 商品情報10
        {
            public int Id;
            public string 名前;
        }
        class 商品販売価格10
        {
            public int Id;
            public string 店名;
        }
        static void test10()
        {
            商品情報10[] 商品情報データ =
            {
                new 商品情報10() { Id=1, 名前="PC-8001" },
                new 商品情報10() { Id=2, 名前="MZ-80K" },
                new 商品情報10() { Id=3, 名前="Basic Master Level-3" },
                new 商品情報10() { Id=4, 名前="COMKIT 8060" },
            };
            商品販売価格10[] 商品販売価格データ =
            {
                new 商品販売価格10() { Id=1, 店名="BitOut" },
                new 商品販売価格10() { Id=1, 店名="富士山音響" },
                new 商品販売価格10() { Id=2, 店名="富士山音響"  },
                new 商品販売価格10() { Id=3, 店名="マイコンセンターROM" },
                new 商品販売価格10() { Id=3, 店名="富士山音響"  },
            };

            var query = from x in 商品情報データ
                        join y in 商品販売価格データ on x.Id equals y.Id into z
                        from a in z.DefaultIfEmpty(new 商品販売価格10() { 店名 = "取扱店なし" })
                        select new { Name = x.名前, 販売店 = a };

            foreach(var 商品 in query)
            {
                Console.WriteLine("{0}", 商品.Name);
                Console.WriteLine("\t{0}", 商品.販売店.店名);
            }
        }


        class 商品情報9
        {
            public int Id;
            public string 名前;
        }
        class 商品販売価格9
        {
            public int Id;
            public int 価格;
            public string 店名;
        }
        static void test9()
        {
            商品情報9[] 商品情報データ9 =
            {
                new 商品情報9() { Id = 1, 名前="PC-8001" },
                new 商品情報9() { Id = 2, 名前="MZ-80K" },
                new 商品情報9() { Id = 3, 名前="Basic Master Level-3" },
            };

            商品販売価格9[] 商品販売価格データ9 =
            {
                new 商品販売価格9() {Id=1, 価格=168000, 店名="BitOut"},
                new 商品販売価格9() {Id=1, 価格=148000, 店名="富士山音響"},
                new 商品販売価格9() {Id=2, 価格=178000, 店名="富士山音響"},
                new 商品販売価格9() {Id=3, 価格=298000, 店名="マイコンセンターROM"},
                new 商品販売価格9() {Id=3, 価格=229000, 店名="富士山音響"},
            };

            var query = from x in 商品情報データ9
                        join y in 商品販売価格データ9
                        on x.Id equals y.Id into z
                        select new { Name = x.名前, 商品データ = z };

            foreach(var 商品 in query)
            {
                Console.WriteLine("{0}", 商品.Name);
                foreach (var 価格情報 in 商品.商品データ)
                    Console.WriteLine("\t{0} {1:C}", 価格情報.店名, 価格情報.価格);
            }
        }

        //http://www.atmarkit.co.jp/fdotnet/extremecs/extremecs_15/extremecs_15_17.html

        class 商品情報2
        {
            private int id;
            public static int ReadCount = 0;

            public int Id
            {
                get { ReadCount++; return id; }
                set { id = value; }
            }
            public string 名前;
            //public int 定価;
        }
        class 商品販売価格
        {
            private int id;
            public static int ReadCount = 0;

            public int Id
            {
                get { ReadCount++; return id; }
                set { id = value; }
            }
            public int 価格;
        }
        static void test8()
        {
            商品情報2[] 商品情報データ =
            {
                 new 商品情報2() { Id = 1, 名前="PC-8001" },
                 new 商品情報2() { Id = 2, 名前="MZ-80K" },
                 new 商品情報2() { Id = 3, 名前="Basic Master Level-3" },
            };
            商品販売価格[] 商品販売価格データ =
            {
                 new 商品販売価格() { Id = 1, 価格 = 148000 },
                 new 商品販売価格() { Id = 2, 価格 = 178000 },
                 new 商品販売価格() { Id = 3, 価格 = 229000 },
            };

#if true
            var query = from x in 商品情報データ
                        join y in 商品販売価格データ 
                        on x.Id equals y.Id
                        select new { Name = x.名前, Price = y.価格 };
#else
            var query = from x in 商品情報データ
                        from y in 商品販売価格データ
                        where x.Id == y.Id
                        select new { Name = x.名前, Price = y.価格 };
#endif

            foreach (var 商品 in query)
            {
                Console.WriteLine("{0} {1:C}", 商品.Name, 商品.Price);
            }
            Console.WriteLine("商品情報.ReadCount={0},商品販売価格.ReadCount={1}",
                商品情報2.ReadCount, 商品販売価格.ReadCount);
        }
        class 商品情報
        {
            public string 名前;
            public string Cpu;
        }
        static void test7()
        {
            商品情報[] 商品情報データ =
            {
                 new 商品情報() { 名前="Altair 680b ", Cpu="6800" },
                 new 商品情報() { 名前="FP-1100", Cpu="Z-80" },
                 new 商品情報() { 名前="H68/TR", Cpu="6800" },
                 new 商品情報() { 名前="LKIT-16", Cpu="MN1610" },
                 new 商品情報() { 名前="MZ-80K", Cpu="Z-80" },
                 new 商品情報() { 名前="TRS-80 Color Computer", Cpu="6809" },
            };

            var query = from n in 商品情報データ group n by n.Cpu;

            foreach(IGrouping<string, 商品情報> r in query)
            {
                Console.WriteLine("CPU={0}", r.Key);

                foreach (商品情報 p in r)
                    Console.WriteLine("\t{0}", p.名前);
            }
        }
        static void test6()
        {
            var query = from n in Enumerable.Range(1, 10)
                        select n * n into m
                        where m > 50
                        select m;
                        //where n * n > 50
                        //select n * n;

            foreach (int n in query)
                Console.WriteLine(n);
        }
        static void test5()
        {
            int[] array = { -2, -1, 0, 1, 2 };

            var query = from x in array
                        //orderby Math.Abs(x) descending
                        orderby Math.Abs(x) descending, x descending
                        select x;

            foreach (int n in query)
                Console.WriteLine(n);
        }
        class Person
        {
            public string 名前;
            public string 性別;
            public string 所属;
        }
        static void test4()
        {
            Person[] persons =
            {
                new Person(){名前="古代進",性別="男",所属="柔道部",},
                new Person(){名前="島大輔",性別="男",所属="ボート部",},
                new Person(){名前="守行",性別="女",所属="パソコン部",},
            };

            var query = 
                from x in persons
                where x.性別 == "男"
                select new { x.名前, x.所属 };

            foreach(var s in query)
            {
                Console.WriteLine("{0} {1}", s.名前, s.所属);
            }
        }
        static void test3()
        {
            var query = from x in Enumerable.Range(1, 9)
                        from y in Enumerable.Range(1, 9)
                        select x + "×" + y + "＝" + (x * y).ToString();
            foreach (string s in query)
                Console.WriteLine(s);
        }
        static void test2()
        {
            int[] array = { 1, 2, 3, };
            var query = from x in array select string.Format("配列の内容は{0}です。", x);
            foreach (string s in query)
                Console.WriteLine(s);
        }
        static void test1()
        {
            string startFolder = @"C:\Intercom\edistation";

            int charsToSkip = startFolder.Length;

            IEnumerable<FileInfo> fileList =
                Directory.GetFiles(startFolder, "*,*",
                SearchOption.AllDirectories).Select(x => new FileInfo(x));

            var queryDupNames =
                from file in fileList
                group file.FullName.Substring(charsToSkip)
                by file.Name into fileGroup
                where fileGroup.Count() > 1
                select fileGroup;

            foreach(var filegroup in queryDupNames)
            {
                foreach(var fileName in filegroup)
                {
                    Console.WriteLine(fileName);
                }
                Console.WriteLine();
            }
        }
    }
}
