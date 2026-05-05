class Solution
{
    //Некое подобие текстового UI и хранение данных между вызовом методов
    static void Main()
    {
        Console.WriteLine(
            "Вас приветствует Великая и Неповторимая Программа для Решения Сравнений По Курсу Дискретной Математики!" +
            "\n(или просто ВНПРСПКДМ)");
        Console.WriteLine(
            "Введите сюда ваше, пока не решенное, сравнение!"
            + "\nФормат ввода(пример): 14546 * х = 7 * a mod 19929" +
            "\nГде x - искомая переменная(то есть всё тот же х) +" +
            "\na - число рождения для получения правостороннего операнда!" +
            "\nВвод:");
        var variables = ComprationSolver.ParseExpression(Console.ReadLine());
        Console.WriteLine(
            $"Итоговое решаемое выражение:{variables["operandBeforeX"]}x = {variables["b"]} (mod {variables["mod"]})" +
            "\nНаходим НОД по Алгоритму Евклида:");
        var q = new List<long>();
        var nod = ComprationSolver.GreatestCommonFactorByEuclid(variables["mod"], variables["operandBeforeX"], ref q);
        Console.WriteLine($"Последний ненулевой остаток(НОД) = {nod}");
        if (ComprationSolver.CheckDivisibility(nod, variables["b"]))
        {
            Console.WriteLine($"\nПроверка делимости коэф. b / НОД:{variables["b"]} mod {nod} = 0✓");
            variables["b"] /= nod;
            variables["operandBeforeX"]/= nod;
            variables["mod"] /= nod;
            Console.WriteLine($"\nСокращаем коэфициенты, получаем следующее сравнение:{variables["operandBeforeX"]}x = {variables["b"]} (mod {variables["mod"]})");
            Console.WriteLine("Алгоритм Евклида для нового сравнения:");
            var newQ = new List<long>();
            ComprationSolver.GreatestCommonFactorByEuclid(variables["mod"], variables["operandBeforeX"], ref newQ);
            if (!q.SequenceEqual(newQ))
            {
                Console.WriteLine("Каким то образом неполные частные не сошлись. Программа останавливается!");
                return;
            }
            Console.WriteLine("В целом сравнение частных нужно при ручном счёте, но эта программа и их проверяет! " +
                "\n\nЧастные сошлись и равны: ");
            ArrToConsole(newQ);
            long PnMinus1 = ComprationSolver.GetPnMinus1Recursive(newQ);
            Console.WriteLine($"\n\nP(n-1) = {PnMinus1}");

            int n = newQ.Count;
            long sign = ((n - 1) % 2 == 0) ? 1 : -1;
            long x0Raw = sign * PnMinus1 * variables["b"];
            long x0 = ((x0Raw % variables["mod"]) + variables["mod"]) % variables["mod"];

            Console.WriteLine($"Частное решение: x0 = {x0}");
            Console.WriteLine("Получаем оставшиеся решения путём прибавления НОД:");
            ComprationSolver.GetAnotherAnswers(x0, variables["mod"], nod);

        }
        else
        {
            Console.WriteLine("\nПроверка делимости коэф. b / НОД:{variables[\"b\"]} mod {nod} = 0\")" +
            "\n Уравнение не имеет решений!");
            return;
        }

        
    }

    //Статический класс с разделением решения на более мелкие шаги, соблюдаем принцип KISS и разделения ответственности, все дела
    static class ComprationSolver
    {
        //14546 * х = 7 * 13 mod 19929 - пример решаемого уравнения, сюда написал чтоб тестить по быстрому
        public static Dictionary<string, long> ParseExpression(string expression)
        {
            var result = new Dictionary<string, long>();

            //парсинг уравнения, выкидываются знаки и всё ненужное для работы программы
            var parsed = expression
                        .Split(new char[] { ' ', '*', '=', 'x', 'х', 'a' }, StringSplitOptions.RemoveEmptyEntries);
            var operands =parsed
                .Where(x => x != "mod" && x != "")
                .Select(x => int.Parse(x))
                .ToArray();

            //заносим все полученные значения в словарь, их легко достать по их названию
            result["operandBeforeX"] = operands[0];
            result["operandBeforeAlpha"] = operands[1];
            result["alpha"] = operands[2];
            result["b"] = operands[2] * operands[1];
            result["mod"] = operands[3];

            return result;
        }

        public static long GreatestCommonFactorByEuclid(long biggest, long less, ref List<long> q)
        {
            Console.WriteLine("Шаг  Делимое  Делитель  Частное   Остаток");
            long remains = int.MaxValue; //остаток
            var i = 1;
            long lastRemains = 0;
            while (remains > 0)
            {
                lastRemains = remains;
                remains = biggest % less;
                Console.WriteLine($"{i,1}   {biggest,4}     {less,4}      {biggest / less,4}       {remains,4}");
                i++;
                q.Add(biggest / less);
                biggest = less;
                less = remains;
            }
            return lastRemains;
        }
        public static bool CheckDivisibility(long nod, long b) => b % nod == 0;

        private static (long Pk, long PkPrev) ComputePRecursive(List<long> q, int k)
        {
            if (k == -1)
                return (Pk: 1, PkPrev: 0);

            var (prevPk, prevPkPrev) = ComputePRecursive(q, k - 1);

            long currentPk = q[k] * prevPk + prevPkPrev;

            string computation = $"{q[k]}·{prevPk} + {prevPkPrev}";
            Console.WriteLine($"{k,2} | {q[k],2} | {currentPk,20} | {computation}");

            return (Pk: currentPk, PkPrev: prevPk);
        }

        public static long GetPnMinus1Recursive(List<long> quotients)
        {
            if (quotients.Count < 2)
                throw new ArgumentException("Нужно минимум 2 неполных частных для вычисления Pₙ₋₁.");

            int targetIndex = quotients.Count - 2;

            Console.WriteLine("\nk | qk | Pk = qk*P(k-1)+P(k-2) | Вычисление");

            var (PnMinus1, _) = ComputePRecursive(quotients, targetIndex);
            return PnMinus1;
        }
        public static void GetAnotherAnswers(long x0, long mod, long nod)
        {
            for (int i = 1; i < nod; i++)
                Console.WriteLine($"x{i} = {x0} + {mod} * {i} = {x0 + mod * i}");
        }
    }
    //метод чтоб красиво вывести массив или список в консоль
    public static void ArrToConsole<T>(IEnumerable<T> arr)
    {
        Console.Write("[");
        foreach (T item in arr) Console.Write($"{item} ");
        Console.Write("]\n");
    }
}