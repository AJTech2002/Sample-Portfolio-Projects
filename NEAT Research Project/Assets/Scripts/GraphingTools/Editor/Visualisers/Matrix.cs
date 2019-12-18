using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Matrix {

	DebuggerHelp d;

	private string matrixName;
	private float emptyConst=-192670830.348739487f;

	public Matrix (int x, int y, string matrixName_) {
		Create (x, y);
		matrixName = matrixName_;
	}

	public float[,] v = new float[1,1];

	//Length of array dimensions
	public int xLen;
	public int yLen;

	public void Create(int x, int y) {
		if (d == null)
		d = GameObject.FindObjectOfType<DebuggerHelp> ();
		xLen = x;
		yLen = y;
		//-------
		v = new float[x, y];

		for (int xr = 0; xr < xLen; xr++) {
			for (int yr = 0; yr < yLen; yr++) {
				v [xr, yr] = emptyConst;
			}
		}

	}

	public void RandomiseValues (float min, float max, bool integers) {
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				if (!integers)
					v [x, y] = Random.Range (min, max);
				else
					v [x, y] = Mathf.RoundToInt(Random.Range (min, max));
			}
		}
	}
		

	public void Transpose () {
		float[,] t = new float[yLen, xLen];
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				t [y, x] = v [x, y];
			}
		}
		int prevXlen = xLen;
		int prevYlen = yLen;
		yLen = prevXlen;
		xLen = prevYlen;
		v = t;
	}

	public Matrix ReturnTranspose () {
		Matrix t = new Matrix(yLen, xLen,matrixName);
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				t.v [y, x] = v [x, y];
			}
		}
		return t;
	}

	public void PrintRow (int n) {
		int x = n;
		string end = "("+matrixName+"):(Row " + n.ToString() + ") \n"+"[";
		for (int y = 0; y < yLen; y++) {
			end += v [x, y]+" ,";
		}
		end += "]";
		d.Log (end);
	}

	public List<float> ReturnRow (int xV) {
		int x = xV;
		List<float> f = new List<float> ();
		for (int y = 0; y < yLen; y++) {
			f.Add (v [x, y]);
		}
		return f;
	}


	public List<float> ReturnColumn (int yV) {
		int y = yV;
		List<float> f = new List<float> ();
		for (int x = 0; x < xLen; x++) {
			f.Add (v [x, y]);
		}
		return f;
	}

	public void PrintColumn (int n) {
		int y = n;
		string end = "("+matrixName+"):(Column " + n.ToString() + ") \n"+"[";
		for (int x = 0; x < xLen; x++) {
			end += v [x, y]+" ,";
		}
		end += "]";
		d.Log (end);
	}


	public void Print() {
		string printString = "";
		int curLine = 0;
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				if (v [x, y] == emptyConst)
					printString += "N,";
				else
				printString += v [x, y].ToString () + ",";
			}
			printString += "\n";
		}
		d.Log ("("+matrixName+"):\n"+printString);
	}

	public void Print (int x, int y) {
		d.Log("("+matrixName+"):"+"Value : " + v[x-1,y-1].ToString() + " (X: " + (x).ToString() + ", Y: " + (y).ToString() +")");
	}

	public float r (int x, int y) {
		if (v [x, y] != null) {
			return v [x, y];
		} else
			return 0;
	}
		
	public Matrix Multiply(Matrix m) {
		return null;
	}

	public void ScalarMultiply (float n) {
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				v [x, y] *= n;
			}
		}
	}

	public List<float> ReturnValues () {
		List<float> r = new List<float> ();
		for (int x = 0; x < xLen; x++) {
			for (int y = 0; y < yLen; y++) {
				r.Add (v [x, y]);
			}
		}
		return r;
	}

	public Matrix ScalarMultiplyRet (float n) {
		Matrix c = this;
		for (int x = 0; x < c.xLen; x++) {
			for (int y = 0; y < c.yLen; y++) {
				c.v [x, y] *= n;
			}
		}
		return c;
	}

	public float derivative (float x) {
		return activate (x) * (1 - activate (x));
	}

	public float activate (float x) {
		return 1 / (1 + Mathf.Exp (-x));
	}

	public void SigmoidActivateMatrix() {
		//EACH ROW
		for (int x = 0; x < xLen; x++) {
			//EACH COLUMN
			for (int y = 0; y < yLen; y++) {
				v [x, y] = activate (v [x, y]);
			}
		}
	}

	public Matrix RetSigmoidActivateMatrix() {
		Matrix c = this;
		//EACH ROW
		for (int x = 0; x < xLen; x++) {
			//EACH COLUMN
			for (int y = 0; y < yLen; y++) {
				c.v [x, y] = activate (c.v [x, y]);
			}
		}

		return c;
	}

	public Matrix RetSigmoidDerivativeMatrix() {
		Matrix c = this;
		//EACH ROW
		for (int x = 0; x < xLen; x++) {
			//EACH COLUMN
			for (int y = 0; y < yLen; y++) {
				c.v [x, y] = derivative (c.v [x, y]);
			}
		}

		return c;
	}

	public void UpdateValue (int x, int y, float val) {
		v [x, y] = val;
	}

	public void Print (string message) {
		d.Log (message);
	}

	public Matrix RetProduct (Matrix b) {
		//B's Rows == A's Columns
		if (b.xLen == yLen || b.yLen == xLen) {
			int newY = b.yLen;
			int newX = xLen;
			Matrix c = new Matrix (newX, newY, "c");
			float retVal = 0;
			for (int i = 0; i < xLen; i++) {
				List<float> rows = ReturnRow (i);
				for (int y = 0; y < b.yLen; y++) {
					List<float> columns = b.ReturnColumn (y);
					if (rows.Count == columns.Count) {
						for (int r = 0; r < rows.Count; r++) {
							retVal += rows [r] * columns [r];
							//Print (" Calc : " + rows [r].ToString () + "x" + columns [r].ToString ());
						}
					}
					c.AddHorizontally (retVal);
					retVal = 0;
					columns.Clear ();
				}

			}
			return c;
		} else {
			Debug.Log(matrixName + ": COLUMNS ( " + yLen + " ) | " + b.matrixName + ": ROWS ( " + b.xLen + " ) ");
			Debug.Log(matrixName + ": ROWS ( " + xLen + " ) | " + b.matrixName + ": COLUMNS ( " + b.yLen + " ) ");
			Debug.LogError ("You need to make sure the columns of the 1st matrix equals the rows of the second matrix!");
		}
			return null;
	}


	public void Product (Matrix b) {
		if (b.xLen == yLen) {
			int newY = b.yLen;
			int newX = b.xLen;
			Matrix c = new Matrix (newX, newY, "c");
			float retVal = 0;
			for (int i = 0; i < xLen; i++) {
				List<float> rows = ReturnRow (i);
				for (int y = 0; y < b.yLen; y++) {
					List<float> columns = b.ReturnColumn (y);
					if (rows.Count == columns.Count) {
						for (int r = 0; r < rows.Count; r++) {
							retVal += rows [r] * columns [r];
							//Print (" Calc : " + rows [r].ToString () + "x" + columns [r].ToString ());
						}
					}
					c.AddHorizontally (retVal);
					retVal = 0;
					columns.Clear ();
				}

			}
			this.v = c.v;
		} else
			Debug.LogError ("You need to make sure the columns of the 1st matrix equals the rows of the second matrix!");
	}


	public void Name (string n) {
		matrixName = n;
	}

	public void PrintDimensions() {
		Debug.Log(matrixName + " : (X: " + xLen + " Y: " + yLen + ")");
	}

	public void PrintDimensions(string name) {
		Debug.Log(name + " : (X: " + xLen + " Y: " + yLen + ")");
	}

	public void AddHorizontally(float val) {
		bool done = false;
		for (int x = 0; x < xLen; x++) {
			if (done == false) {
				for (int y = 0; y < yLen; y++) {
					if (done == false) {
						if (v [x, y] == emptyConst) {
							v [x, y] = val;
							done = true;
							break;
						} else
							continue;
					}
				}
			} else
				break;
		}
		return;
	}

	public Matrix RetOpposite(Matrix b) {
		if (b.xLen == xLen && b.yLen == yLen) {
			Matrix c = new Matrix (xLen, yLen, "Return Matrix");
			for (int x = 0; x < xLen; x++) {
				for (int y = 0; y < yLen; y++) {
					c.v [x, y] = -v [x, y];
				}
			}
		} else
			Debug.LogError ("Not same length :( ");
		return null;
	}

	public float SumOfContainingValues() {
		List<float> f = ReturnValues ();
		float finalSum = 0;
		for (int i = 0; i < f.Count; i++) {
			finalSum += f [i];
		}
		return finalSum;
	}

	public void Add (Matrix b) {

		if (b.xLen == xLen && b.yLen == yLen) {
				
			for (int x = 0; x < xLen; x++) {
				for (int y = 0; y < yLen; y++) {
					v [x, y] += b.v [x, y];
				}
			}

		} else {
			Debug.LogError ("Length of Matricies have to be the same");
		}
			
	}

	public Matrix RetAdd (Matrix b) {

		Matrix c = this;

		if (b.xLen == xLen && b.yLen == yLen) {

			for (int x = 0; x < xLen; x++) {
				for (int y = 0; y < yLen; y++) {
					c.v [x, y] += b.v [x, y];
				}
			}

		}

		return c;

	}


	public void ClearLog() {
		d.Clear ();
	}


	public Matrix HalfCombine (Matrix b) {
		if (xLen == b.xLen && yLen == b.yLen) {
			Matrix c = new Matrix (xLen, yLen, "C");
			for (int x = 0; x < xLen; x++) {
				for (int y = 0; y < yLen; y++) {
					if (x > Mathf.RoundToInt (xLen / 2)) {
						c.AddHorizontally (b.v [x, y]);
					} else {
						c.AddHorizontally (v [x, y]);
					}
				}
			}
			if (c.xLen != xLen || c.yLen != yLen)
				Debug.LogError ("Not returning same val");
			return c;

		} else
			return null;
	}

	public Matrix AverageCombine (Matrix b) {
		if (xLen == b.xLen && yLen == b.yLen) {
			Matrix c = new Matrix (xLen, yLen, "C");
			for (int x = 0; x < xLen; x++) {
				for (int y = 0; y < yLen; y++) {
					float cV = (b.v [x, y] + v [x, y])/2;
					c.AddHorizontally (cV);
				}
			}
			if (c.xLen != xLen || c.yLen != yLen)
				Debug.LogError ("Not returning same val");
			return c;

		} else
			return null;
	}


}

/*
 * 
 * //EACH ROW
for (int x = 0; x < xLen; x++) {
	//EACH COLUMNS
	for (int y = 0; y < yLen; y++) {

	}
}
	*/