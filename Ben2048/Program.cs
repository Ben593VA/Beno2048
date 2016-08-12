using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ben2048
{
    class Program
    {
        static void Main(string[] args)
        {
            Ben2048 game = new Ben2048();
            game.Run();
        }
    }

    class Ben2048
    {

        public ulong Score { get; private set; }
        public ulong[,] Plateau { get; private set; }

        private readonly int nRows;
        private readonly int nCols;
        private readonly Random random = new Random();

        public Ben2048()
        {
            this.Plateau = new ulong[4, 4];
            this.nRows = this.Plateau.GetLength(0);
            this.nCols = this.Plateau.GetLength(1);
            this.Score = 0;
        }

        public void Run()
        {
            bool hasUpdated = true;
            do
            {
                if (hasUpdated)
                {
                    NouvelleValeur();
                }

                Affichage();

                if (IsDead())
                {
                    using (new ColorOutput(ConsoleColor.Red))
                    {
                        Console.WriteLine("BOUHHH !!!");
                        break;
                    }
                }

                Console.WriteLine("Utilisez les flèches pour déplacer les tuiles. Appuyez sur Ctrl-C pour quitter.");
                ConsoleKeyInfo input = Console.ReadKey(true);
                Console.WriteLine(input.Key.ToString());

                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        hasUpdated = Update(Direction.Up);
                        break;

                    case ConsoleKey.DownArrow:
                        hasUpdated = Update(Direction.Down);
                        break;

                    case ConsoleKey.LeftArrow:
                        hasUpdated = Update(Direction.Left);
                        break;

                    case ConsoleKey.RightArrow:
                        hasUpdated = Update(Direction.Right);
                        break;

                    default:
                        hasUpdated = false;
                        break;
                }
            }
            while (true);

            Console.WriteLine("Appuyez sur une touche pour quitter.");
            Console.Read();
        }

        private static ConsoleColor GetNumberColor(ulong num)
        {
            switch (num)
            {
                case 0:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Cyan;
                case 4:
                    return ConsoleColor.Magenta;
                case 8:
                    return ConsoleColor.Red;
                case 16:
                    return ConsoleColor.Green;
                case 32:
                    return ConsoleColor.Yellow;
                case 64:
                    return ConsoleColor.DarkYellow;
                case 128:
                    return ConsoleColor.DarkCyan;
                case 256:
                    return ConsoleColor.Blue;
                case 512:
                    return ConsoleColor.DarkMagenta;
                case 1024:
                    return ConsoleColor.DarkBlue;
                default:
                    return ConsoleColor.DarkRed;
            }
        }

        private static bool Update(ulong[,] plateau, Direction direction, out ulong score)
        {
            int nRows = plateau.GetLength(0);
            int nCols = plateau.GetLength(1);

            score = 0;
            bool hasUpdated = false;

            // Booléen pour définir si l'on se déplace dans les colonnes ou dans les lignes

            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

            int outterCount = isAlongRow ? nRows : nCols;
            int innerCount = isAlongRow ? nCols : nRows;
            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;

            Func<int, int> drop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex - 1)
                : new Func<int, int>(innerIndex => innerIndex + 1);

            Func<int, int> reverseDrop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex + 1)
                : new Func<int, int>(innerIndex => innerIndex - 1);

            Func<ulong[,], int, int, ulong> getValue = isAlongRow
                ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]);

            Action<ulong[,], int, int, ulong> setValue = isAlongRow
                ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v);

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(plateau, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }

                    while (innerCondition(newJ) && getValue(plateau, i, newJ) == 0);

                    if (innerCondition(newJ) && getValue(plateau, i, newJ) == getValue(plateau, i, j))
                    {
                        // On fusionne les valeurs si on percute une valeur équivalente et s'il n'y a pas de fusion précédente

                        ulong newValue = getValue(plateau, i, newJ) * 2;
                        setValue(plateau, i, newJ, newValue);
                        setValue(plateau, i, j, 0);

                        hasUpdated = true;
                        score += newValue;
                    }
                    else
                    {
                        // Si la limite est atteinte, si on percute une valeur différente ou s'il y'a eu une précedente fusion, on stocke la valeur au bout

                        // Définir la position

                        newJ = reverseDrop(newJ);

                        // Mettre à jour la valeur

                        if (newJ != j)
                        {
                            hasUpdated = true;
                        }

                        ulong value = getValue(plateau, i, j);
                        setValue(plateau, i, j, 0);
                        setValue(plateau, i, newJ, value);
                    }
                }
            }

            return hasUpdated;
        }

        private bool Update(Direction dir)
        {
            ulong score;
            bool isUpdated = Ben2048.Update(this.Plateau, dir, out score);
            this.Score += score;
            return isUpdated;
        }

        private bool IsDead()
        {
            // Aucune direction ne fonctionne

            ulong score;
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                ulong[,] clone = (ulong[,])Plateau.Clone();
                if (Ben2048.Update(clone, dir, out score))
                {
                    return false;
                }
            }

            return true;
        }

        private void Affichage()
        {
            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    using (new ColorOutput(Ben2048.GetNumberColor(Plateau[i, j])))
                    {
                        Console.Write(string.Format("{0,6}", Plateau[i, j]));
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("Score: {0}", this.Score);
            Console.WriteLine();
        }

        private void NouvelleValeur()
        {
            // La partie n'est pas finie donc il doit rester au moins 1 emplacement vide

            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();

            for (int iRow = 0; iRow < nRows; iRow++)
            {
                for (int iCol = 0; iCol < nCols; iCol++)
                {
                    if (Plateau[iRow, iCol] == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }

            // Choisir au hasard un emplacement vide

            int iSlot = random.Next(0, emptySlots.Count);

            // Choisir au hasard 2 avec 95% de chance ou 4

            ulong value = random.Next(0, 100) < 95 ? (ulong)2 : (ulong)4;

            Plateau[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2] = value;
        }

        #region Utility Classes

        enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }

        class ColorOutput : IDisposable
        {
            public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }

            public void Dispose()
            {
                Console.ResetColor();
            }
        }

        #endregion Utility Classes
    }
}
