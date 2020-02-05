using Api.Managers;
using NLog;

namespace BotCore
{
    class KeyBoardBuilder
    {
        public static int CallcRows(int count, int col)
        {
            return count % col == 0 ? count / col : count / col + 1;
        }


        public static T[] CreateCol<T>(int count) where T : new()
        {
            Logger Log = new ApiLogManager().GetManager<KeyBoardBuilder>();
            T[] col = new T[count];

            for (int i = 0; i < count; i++)
            {
                col[i] = new T();
            }

            Log.Info($"Create col {col}");
            return col;
        }

        public static T[][] CreateKeyBoard<T>(int colNum, int num) where T : new()
        {
            Logger Log = new ApiLogManager().GetManager<KeyBoardBuilder>();
            int rowNum = CallcRows(num, colNum);
            T[][] keyBoard = new T[rowNum][];

            for (int i = 0; i < rowNum; i++)
            {
                colNum = colNum > num ? num : colNum;
                keyBoard[i] = CreateCol<T>(colNum);
                num -= colNum;
            }
            Log.Info($"Create keyBoard {keyBoard}");
            return keyBoard;
        }
    }
}
