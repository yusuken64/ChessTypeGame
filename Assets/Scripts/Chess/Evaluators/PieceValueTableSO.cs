using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceValueTable", menuName = "Chess/PieceValueTable", order = 1)]
public class PieceValueTableSO : ScriptableObject
{
    public PieceType pieceType;
    public string pieceSquareCsv;  // CSV representation of the piece-square table
    public int[,] pieceSquareTable;

    // Constructor to initialize the piece-square table
    public void Initialize(int[,] table)
    {
        pieceSquareTable = table;
        pieceSquareCsv = ConvertToCsv(table);
    }

    // Optionally, you could have a method to get a value for a particular square
    public int GetValueForSquare(int x, int y)
    {
        return pieceSquareTable[x, y];
    }

    // Convert piece-square table (2D array) to CSV string
    public string ConvertToCsv(int[,] table)
    {
        if (table is null) table = new int[8, 8];

        System.Text.StringBuilder csv = new System.Text.StringBuilder();

        // Convert the 2D array to CSV string
        for (int j = 0; j < table.GetLength(1); j++)  // Loop over columns
        {
            for (int i = 0; i < table.GetLength(0); i++)  // Loop over rows
            {
                csv.Append(table[i, j]);
                csv.Append(",");
            }

            // Append a newline after each row
            csv.AppendLine();
        }

        return csv.ToString().TrimEnd();  // Remove last newline character
    }

    [ContextMenu("Get String")]
    public void GetPieceSquareString()
    {
        pieceSquareCsv = ConvertToCsv(pieceSquareTable);
    }

    // Convert CSV string back to a 2D piece-square table (int[,])
    [ContextMenu("Set String")]
    public void SetPieceSquareFromCsv()
    {
        if (!string.IsNullOrEmpty(pieceSquareCsv))
        {
            // Split the CSV string into rows
            pieceSquareTable = AsTable();
        }
        else
        {
            Debug.LogError("PieceSquareCsv is empty or null");
        }
    }

    private int[,] tableCache;

    public int[,] AsCachedTable()
    {
        if (tableCache == null)
        {
            tableCache = AsTable();
        }

        return tableCache;
    }

    public int[,] AsTable()
    {
        string sanitizedCsv = pieceSquareCsv.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        int size = 8;  // Assuming the table is 8x8
        var table = new int[size, size];

        string[] values = sanitizedCsv.Split(',');
        for (int i = 0; i < values.Length; i++)
        {
            if (i >= size * size) { break; }
            int x = i % 8;
            int y = i / 8;
            table[x, size - y - 1] = int.Parse(values[i]);
        }

        return table;
    }
}
