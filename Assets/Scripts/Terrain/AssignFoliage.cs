using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Fix trees from spawning on steep terrain
 * Add height to the grass and tree algorithms so they grow
 * less at higher altitudes, trees might require changing the way they
 * are added currently
 */
public class AssignFoliage : MonoBehaviour {

	public GameObject[] trees = new GameObject[10];
	private TerrainData terrainData;
	private int terrainWidth, terrainHeight;
	private bool[,] allTrees = new bool[1025,1025];



	void Start() {
		Foliage();
	}

	/*void OnGUI() {
		if (GUI.Button(new Rect(340,10,100,50),"Foil")) {
			Foil();
		}
	}*/

	void Foliage() {
		
		Terrain terrain = GetComponent<Terrain>();
		terrainData = terrain.terrainData;
		int[,] detailMapData = new int[terrainData.detailWidth, terrainData.detailHeight];
		float height;
		float steepness;
		float x_01, y_01;
		float valuef;
		int value;

		for (int y = 0; y < terrainData.detailHeight; y++) {
			for (int x = 0; x < terrainData.detailWidth; x++) {
				// Normalise x/y coordinates to range 0-1 
				y_01 = (float)y/(float)terrainData.detailHeight;
				x_01 = (float)x/(float)terrainData.detailWidth;

				height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );
				steepness = terrainData.GetSteepness(y_01,x_01);

				//Foilage
				value = 0;
				valuef = 0f;
				if (height > 145f) {
					valuef = 1f - steepness / 90f;
					value = Mathf.RoundToInt(valuef);
				}

				if (value == 1) {
					value *= Random.Range(0,3);
					if (value < 2)
						value = 0;
				}
				//value = value - Mathf.RoundToInt(height/100f);

				detailMapData[x, y] = value;
			}
		}
		terrainData.SetDetailLayer(0,0,0,detailMapData);

		//Trees
		//Puut ei perkele vieläkään mene maahan asti aina ja välillä menee läpi...
		int maxTrees = 350;
		int treeCount = 0;
		Vector3 pos = Vector3.zero;
		float ypos = 0f;
		GameObject tree;
		int iteration = 0;
		treeCount = 0;
		GameObject treeContainer = new GameObject("Trees");
		float[,,] alphas = terrainData.GetAlphamaps(0,0,terrainData.alphamapWidth, terrainData.alphamapHeight);
		while (treeCount < maxTrees) {
			
			if (iteration > 1000)
				break;
			
			pos.x = Random.Range(1, 1000); //1 ... 999
			pos.z = Random.Range(1, 1000);

			x_01 = pos.x / terrainData.heightmapWidth;
			y_01 = pos.z / terrainData.heightmapHeight;

			if (alphas[Mathf.FloorToInt(pos.z*terrainData.alphamapWidth/terrainData.heightmapWidth),Mathf.FloorToInt(pos.x*terrainData.alphamapHeight/terrainData.heightmapWidth),1] == 1.0f) continue;

			if ( allTrees[(int)pos.x, (int)pos.z] ) continue;

			ypos = terrain.SampleHeight(pos);
			if (ypos <= 145f) continue;
			pos.y = ypos;

			tree = trees[Random.Range(0,trees.Length)];
			GameObject newtree = (GameObject)Instantiate(tree,pos,Quaternion.identity);
			newtree.name = "Tree";
			newtree.transform.parent = treeContainer.transform;

			allTrees[(int)pos.x, (int)pos.z] = true;

			treeCount++;
		}
	}

	bool CheckTreeDistance(TreeInstance tree, List<TreeInstance> list) {
		foreach (TreeInstance i in list) {
			float d = Vector3.Distance(i.position, tree.position);
			if (d <= 15f/1000f ) 
				return false;
		}

		return true;
	}

	//Returns the height in a given location on the terrain
	//We divide the height by 0.95 to make sure the tree isn't floating in the air
	float GetHeightInLocation(Vector3 pos, TerrainData data) {
		int x = (int)pos.x;
		int z = (int)pos.z;
		float h = data.GetHeight(x,z);
		return h;
	}
}
