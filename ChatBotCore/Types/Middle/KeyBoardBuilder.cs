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
			T[] col = new T[count];

			for (int i = 0; i < count; i++)
			{
				col[i] = new T();
			}

			return col;
		}

		public static T[][] CreateKeyBoard<T>(int colNum, int num) where T : new()
		{
			int rowNum = CallcRows(num, colNum);
			T[][] keyBoard = new T[rowNum][];

			for (int i = 0; i < rowNum; i++)
			{
				colNum = colNum > num ? num : colNum;
				keyBoard[i] = CreateCol<T>(colNum);
				num -= colNum;
			}
			return keyBoard;
		}
	}
}
