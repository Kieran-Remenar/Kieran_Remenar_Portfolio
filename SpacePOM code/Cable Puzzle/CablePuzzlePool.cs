using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CablePuzzlePool
{
	private int m_matrixSize;
	private List<List<char>> m_activeMatrix;
	private List<Cable> m_cables;

	public List<List<char>> ActiveMatrix { get => m_activeMatrix; }
	public List<Cable> Cables { get => m_cables; }

	public void Initialize(int size)
	{
		m_matrixSize = size;
		m_activeMatrix = new List<List<char>>();
		m_cables = new List<Cable>();

		if (m_matrixSize == 4) //Holds the puzzles for 'easy' setting
		{
			m_cables.Add(new Cable(0, Color.red));
			m_cables.Add(new Cable(1, Color.green));

			int rand = Random.Range(0, 5);
			switch (rand)
			{
				case 0:
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'g' });
					m_activeMatrix.Add(new List<char> { 'r', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'g', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'n', 'r', 'n', 'n' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(4);
					break;
				case 1:
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'g', 'w', 'g' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'r', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'r' });
					m_cables[0].SetLength(2);
					m_cables[1].SetLength(4);
					break;
				case 2:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'r', 'w' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'r', 'g', 'n', 'g' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'w', 'n' });
					m_cables[0].SetLength(4);
					m_cables[1].SetLength(2);
					break;
				case 3:
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'g' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'r', 'g', 'w', 'w' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'r', 'w' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(4);
					break;
				case 4:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'g', 'n' });
					m_activeMatrix.Add(new List<char> { 'g', 'w', 'w', 'r' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'r', 'n', 'n' });
					m_cables[0].SetLength(4);
					m_cables[1].SetLength(3);
					break;
				default: break;
			}
		}
		else if (m_matrixSize == 6) //Holds the puzzles for 'medium' setting
		{
			m_cables.Add(new Cable(0, Color.red));
			m_cables.Add(new Cable(1, Color.green));
			m_cables.Add(new Cable(2, Color.yellow));

			int rand = Random.Range(0, 3);
			switch (rand)
			{
				case 0:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'y', 'w', 'n', 'r', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'y', 'w', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'g', 'g', 'r', 'n', 'n' });
					m_cables[0].SetLength(6);
					m_cables[1].SetLength(1);
					m_cables[2].SetLength(3);
					break;
				case 1:
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'g', 'n', 'y' });
					m_activeMatrix.Add(new List<char> { 'g', 'n', 'r', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'y', 'r', 'n', 'w' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(4);
					m_cables[2].SetLength(6);
					break;
				case 2:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'y', 'w', 'r' });
					m_activeMatrix.Add(new List<char> { 'n', 'g', 'w', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n', 'y', 'r' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'g', 'n', 'n', 'n' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(6);
					m_cables[2].SetLength(4);
					break;
				default: break;
			}
		}
		else if (m_matrixSize == 8) //Holds the puzzles for 'hard' setting
		{
			m_cables.Add(new Cable(0, Color.red));
			m_cables.Add(new Cable(1, Color.green));
			m_cables.Add(new Cable(2, Color.yellow));
			m_cables.Add(new Cable(3, Color.blue));

			int rand = Random.Range(0, 5);
			switch (rand)
			{
				case 0:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'b', 'w', 'g', 'n', 'w', 'y', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'w', 'n', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'n', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'y', 'n', 'w', 'g', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'r', 'n', 'w', 'n', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'n', 'r', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'b', 'n', 'n', 'n', 'n', 'n', 'n', 'n' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(6);
					m_cables[2].SetLength(8);
					m_cables[3].SetLength(7);
					break;
				case 1:
					m_activeMatrix.Add(new List<char> { 'g', 'n', 'n', 'n', 'n', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'w', 'n', 'n', 'y', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'w', 'n', 'n', 'w', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'y', 'n', 'n', 'n', 'w', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'w', 'g', 'b', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'w', 'n', 'n', 'w', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'n', 'n', 'w', 'b', 'w' });
					m_activeMatrix.Add(new List<char> { 'r', 'n', 'w', 'r', 'n', 'n', 'n', 'n' });
					m_cables[0].SetLength(5);
					m_cables[1].SetLength(7);
					m_cables[2].SetLength(6);
					m_cables[3].SetLength(4);
					break;
				case 2:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'w', 'n', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'n', 'y', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n', 'n', 'n', 'r', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'w', 'r', 'n', 'b', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'b', 'n', 'n', 'n', 'n', 'n', 'w' });
					m_activeMatrix.Add(new List<char> { 'w', 'n', 'n', 'n', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'n', 'n', 'n', 'w', 'n', 'g' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'y', 'n', 'n', 'g', 'w', 'n' });
					m_cables[0].SetLength(4);
					m_cables[1].SetLength(7);
					m_cables[2].SetLength(8);
					m_cables[3].SetLength(5);
					break;
				case 3:
					m_activeMatrix.Add(new List<char> { 'b', 'n', 'n', 'w', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'w', 'n', 'n', 'n', 'y' });
					m_activeMatrix.Add(new List<char> { 'g', 'n', 'g', 'w', 'n', 'r', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'w', 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'w', 'w', 'n', 'n', 'r' });
					m_activeMatrix.Add(new List<char> { 'n', 'b', 'n', 'n', 'w', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'w', 'n', 'n', 'y' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'w', 'n', 'n', 'n' });
					m_cables[0].SetLength(4);
					m_cables[1].SetLength(2);
					m_cables[2].SetLength(7);
					m_cables[3].SetLength(6);
					break;
				case 4:
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'w', 'n', 'n', 'y' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'n', 'w', 'b', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'w', 'n', 'n', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'r', 'w', 'n', 'y', 'w', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'n', 'n', 'r', 'n', 'n', 'n', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'w', 'n', 'n', 'n', 'n', 'b' });
					m_activeMatrix.Add(new List<char> { 'n', 'w', 'w', 'n', 'n', 'g', 'w', 'n' });
					m_activeMatrix.Add(new List<char> { 'n', 'g', 'n', 'n', 'n', 'n', 'n', 'n' });
					m_cables[0].SetLength(3);
					m_cables[1].SetLength(5);
					m_cables[2].SetLength(8);
					m_cables[3].SetLength(6);
					break;
				default: break;
			}
		}
		else
		{
			Debug.LogError("Incorrect matrix size passed; must be either 4, 6, or 8.");
		}
	}
}
