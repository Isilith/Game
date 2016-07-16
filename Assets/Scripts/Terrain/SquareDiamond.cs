using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareDiamond : MonoBehaviour {

	private int t_width, t_height;
	private float[,] heights;
	private TerrainData terrainData;

	public Texture2D tex;
	public int seed = 0;


	void Start() {
		Terrain terrain = GetComponent<Terrain>();
		terrainData = terrain.terrainData;

		t_width = terrainData.heightmapWidth;
		t_height = terrainData.heightmapHeight;
	}

	void OnGUI() {
		if (GUI.Button(new Rect(10,10,100,50),"Run")) {
				Random.seed = seed;
			heights = terrainData.GetHeights(0,0,t_width, t_height);//new float[t_width, t_height];
			GenerateHeightmap();
			terrainData.SetHeights(0,0,heights);


		}
		if (GUI.Button(new Rect(120,10,100,50),"Smooth")) {
			heights = terrainData.GetHeights(0,0,t_width, t_height);
			SmoothHeightmap();
			terrainData.SetHeights(0,0,heights);
		}
		if (tex != null)
			GUI.DrawTexture(new Rect(Screen.width-200f,0f,200,200), tex);
	}



	/*
	 * This is supposed to smooth the terrain only in the areas where 
	 * it's not too steep (rocky). Doesn't seem to work right now...
	 * One solution could be to write a custom GetSteepness function
	 * Also try to use something else than the average value for the new
	 * heights, might make the terrain smoother.
	 */
	void SmoothHeightmap() {
		//float steepness;
		//float y_01;
		//float x_01;
		float[] neighbours = new float[9];
		int count = 0;
		//float[,,] alphas = terrainData.GetAlphamaps(0,0,terrainData.alphamapWidth, terrainData.alphamapHeight);
		//float alpha;
		for (int y = 0; y < t_height; y++) {
			for (int x = 0; x < t_width; x++) {
				count = 0;
				//y_01 = (float)y/(float)t_height;
				//x_01 = (float)x/(float)t_width;

				//steepness = terrainData.GetSteepness(y_01,x_01);
				//alpha = alphas[Mathf.FloorToInt(x*terrainData.alphamapWidth/t_width),Mathf.FloorToInt(y*terrainData.alphamapHeight/t_height),1];
				//if ( alpha < 1.0f ) {
					
					neighbours = GetNeighbours(x,y);
					foreach(float f in neighbours)
						if (f != 0)
							count++;
					float avg = neighbours.Sum() / count;
					float diff = heights[x,y] - avg;
					heights[x,y] -= diff;
				//}
			}
		}


	}

	float[] GetNeighbours(int x, int y) {
		float[] ret = new float[9];
		int ind = 0;

		if (x > 0 && x < t_width-1)
		{
			if (y > 0 && y < t_height-1) 
			{
				//anything but the sides (red)
				for (int i=-1; i<=1; i++)
					for (int j=-1; j<=1; j++) 
						//if (i != 0 && j!=0)
						ret[ind++] = heights[x+i, y+j];
			}
			//handle all the sides, excluding corners (orange)
			else if (y == 0)
			{
				ind = 0;
				//bottom side
				for (int i=0; i<=1; i++)
					for (int j=0; j<=1; j++) 
						//if (i != 0 && j!=0)
						ret[ind++] = heights[x+i, y+j];
			}
			else if (y == t_height-1)
			{
				ind = 0;
				//top side
				for (int i=-1; i<=1; i++)
					for (int j=-1; j<=0; j++) 
						//if (i != 0 && j!=0)
						ret[ind++] = heights[x+i, y+j];
			}
		}
		else if (y > 0 && y < t_height-1)
		{
			if (x == 0)
			{
				ind = 0;
				//left side
				for (int i=0; i<=1; i++)
					for (int j=-1; j<=1; j++) 
						//if (i != 0 && j!=0)
						ret[ind++] = heights[x+i, y+j];
			}
			else if (x == t_width-1)
			{
				ind = 0;
				//right side
				for (int i=-1; i<=0; i++)
					for (int j=-1; j<=1; j++) 
						//if (i != 0 && j!=0)
						ret[ind++] = heights[x+i, y+j];
			}
		}
		//handle corners (green)
		else if (x == 0 && y == 0)
		{
			ind = 0;
			//bottom left
			for (int i=0; i<=1; i++)
				for (int j=0; j<=1; j++) 
					//if (i != 0 && j!=0)
					ret[ind++] = heights[x+i, y+j];
		}
		else if (x == t_width-1 && y == 0)
		{
			ind = 0;
			//bottom right
			for (int i=-1; i<=0; i++)
				for (int j=0; j<=1; j++) 
					//if (i != 0 && j!=0)
					ret[ind++] = heights[x+i, y+j];
		}
		else if (x == 0 && y == t_height)
		{
			ind = 0;
			//top left
			for (int i=0; i<=1; i++)
				for (int j=-1; j<=0; j++) 
					//if (i != 0 && j!=0)
					ret[ind++] = heights[x+i, y+j];
		}
		else if (x == t_width && y == t_height)
		{
			ind = 0;
			//top left
			for (int i=-1; i<=0; i++)
				for (int j=-1; j<=0; j++) 
					//if (i != 0 && j!=0)
					ret[ind++] = heights[x+i, y+j];
		}
		return ret;
	}

	float myRandom(float r) {
		return 2*r*Random.value - r;
	}
		
	void Fractal(float div, int a, int b, int c, int d) {
		if (b - a <= 1)
			return;

		int a_w = a % t_width;
		int a_h = a / t_height;
		int b_w = b % t_width;
		int b_h = b / t_height;
		int c_w = c % t_width;
		int c_h = c / t_height;
		int d_w = d % t_width;
		int d_h = d / t_height;

		int mid = ( a + d ) / 2;
		int mid_w = mid % t_width;
		int mid_h = mid / t_height;
	
		heights[mid_w, mid_h] = heights[mid_w, mid_h] != 0.5f ? heights[mid_w, mid_h] : (heights[a_w, a_h] + heights[b_w ,b_h] + heights[c_w, c_h] + heights[d_w, d_h]) / 4 + myRandom(div);

		heights[mid_w, mid_h] = Mathf.Min(1.0f, heights[mid_w, mid_h]);

		int top = ( c + d ) / 2;
		int bot = ( a + b ) / 2;
		int lef = ( a + c ) / 2;
		int rig = ( b + d ) / 2;

		int top_w = top % t_width;
		int top_h = top / t_height;
		int bot_w = bot % t_width;
		int bot_h = bot / t_height;
		int lef_w = lef % t_width;
		int lef_h = lef / t_height;
		int rig_w = rig % t_width;
		int rig_h = rig / t_height;



		heights[top_w, top_h] = heights[top_w, top_h] != 0.5f ? heights[top_w, top_h] : (heights[c_w, c_h] + heights[d_w, d_h]) / 2 + myRandom(div);
		heights[bot_w, bot_h] = heights[bot_w, bot_h] != 0.5f ? heights[bot_w, bot_h] : (heights[a_w, a_h] + heights[b_w, b_h]) / 2 + myRandom(div);
		heights[lef_w, lef_h] = heights[lef_w, lef_h] != 0.5f ? heights[lef_w, lef_h] : (heights[a_w, a_h] + heights[c_w, c_h]) / 2 + myRandom(div);
		heights[rig_w, rig_h] = heights[rig_w, rig_h] != 0.5f ? heights[rig_w, rig_h] : (heights[b_w, b_h] + heights[d_w, d_h]) / 2 + myRandom(div);

		heights[top_w, top_h] = Mathf.Min(1.0f, heights[top_w, top_h]);
		heights[bot_w, bot_h] = Mathf.Min(1.0f, heights[bot_w, bot_h]);
		heights[lef_w, lef_h] = Mathf.Min(1.0f, heights[lef_w, lef_h]);
		heights[rig_w, rig_h] = Mathf.Min(1.0f, heights[rig_w, rig_h]);

		div /= 2f;
		Fractal(div,   a, bot, lef, mid);
		Fractal(div, bot,   b, mid, rig);
		Fractal(div, lef, mid,   c, top);
		Fractal(div, mid, rig, top,   d);
	}

	void GenerateHeightmap() {
		int a = 0;
		int b = t_width-1;
		int c = b*t_height;
		int d = t_width*t_height-1;
		float div = .5f;
		Fractal(div, a, b, c, d);
	}

}
