using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Boids
{
    class InfluenceMap
    {
        int mapWidth, mapHeight;
        public int tileSize;
        int divider = 5;
        float neighbourWeight = 1.4f;
        Tile[,] tiles;
        List<Boid>[,] boidLists;

        public InfluenceMap()
        {}

        public void Update(Boid[] boids)
        {
            ResetCells();
            RegisterAllBoids(boids);
        }

        private void ResetCells()
        {
            foreach (List<Boid> l in boidLists)
            {
                l.Clear();
            }
        }

        public void Initialize(int mapWidth, int mapHeight, int cellSize)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.tileSize = cellSize;
            boidLists = new List<Boid>[mapWidth/cellSize, mapHeight / cellSize];
            tiles = new Tile[mapWidth / cellSize, mapHeight / cellSize];

            for (int i = 0; i < boidLists.GetLength(0); i++)
            {
                for (int j = 0; j < boidLists.GetLength(1); j++)
                {
                    tiles[i, j] = new Tile(cellSize, new Vector2(i*cellSize, j * cellSize));
                    boidLists[i, j] = new List<Boid>();
                }
            }
        }

        public void RegisterBoid(Boid boid)
        {
            boidLists[(int)(boid.Center.X / tileSize), (int)(boid.Center.Y / tileSize)].Add(boid);
        }

        public void RegisterAllBoids(Boid[] allBoids)
        {
            for (int i = 0; i < Game1.numberOfBoids; i++)
            {
                //Within Array bounds check before adding to list
                if (allBoids[i].Center.X < tileSize * tiles.GetLength(0) && allBoids[i].Center.X > 0 && allBoids[i].Center.Y > 0 &&
                    allBoids[i].Center.Y < tileSize * tiles.GetLength(1))
                {
                    boidLists[(int)(allBoids[i].Center.X / tileSize), (int)(allBoids[i].Center.Y / tileSize)].Add(allBoids[i]);
                }
            }

        }

        public List<Boid> GetBoidsInCell(int i, int j)
        {
            if (i < boidLists.GetLength(0) && i > 0 && j > 0 && j < boidLists.GetLength(0))
            {
                return boidLists[i, j];
            }
            return new List<Boid>();
        }

        public List<Boid> GetBoidsInCell(Vector2 pos)
        {
            //Within bounds check of pos before returning list. If out of bounds return an empty list
            if (pos.X > 0 && pos.Y > 0 && pos.X < mapWidth && pos.Y < mapHeight)
            {
                return boidLists[(int)(pos.X / tileSize), (int)(pos.Y / tileSize)];
            }
            return new List<Boid>();
        }

        public List<Tuple<int,int>> GetSurroundingTiles(int i, int j, int tilesStepsAway)
        {
            List<Tuple<int, int>> surroundingTiles = new List<Tuple<int, int>>();

            for (int k = -tilesStepsAway; k < tilesStepsAway; k++)
            {
                for (int l = -tilesStepsAway; l < tilesStepsAway; l++)
                {
                    if (WithinMapBounds(i + k, j + l))
                    {
                        surroundingTiles.Add(new Tuple<int, int>(i + k, j + l));
                    }
                }
            }
            return surroundingTiles;
        }

        private bool WithinMapBounds(int i, int j)
        {
            if (i >= 0 && i < tiles.GetLength(0) && j >= 0 && j < tiles.GetLength(1))
            {
                return true;
            }

            return false;
        }

        public void DrawGrid(SpriteBatch sb)
        {
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    tiles[i, j].Draw(sb, CalculateColor(i, j));
                }
            }
        }

        public void DrawNeighbourTiles(SpriteBatch sb, int i, int j, int stepsAway)
        {
            List<Tuple<int,int>> indices = GetSurroundingTiles(i, j, stepsAway);

            foreach (Tuple<int,int> t in indices)
            {
                tiles[t.Item1, t.Item2].Draw(sb, Color.Black);
            }
        }

        private Color CalculateColor(int i, int j)
        {
            float value = (boidLists[i, j].Count);

            if(i < boidLists.GetLength(0) - 1)
            {
                value += (boidLists[i + 1, j].Count) / neighbourWeight;
            }
            if (i > 0)
            {
                value += (boidLists[i - 1, j].Count) / neighbourWeight;
            }
            if (j < boidLists.GetLength(1) - 1)
            {
                value += (boidLists[i, j + 1].Count) / neighbourWeight;
            }
            if (j > 0)
            {
                value += (boidLists[i, j - 1].Count) / neighbourWeight;
            }
            if (i > 0 && j > 0)
            {
                value += (boidLists[i - 1, j - 1].Count) / neighbourWeight;
            }
            if (i < boidLists.GetLength(0) - 1 && j < boidLists.GetLength(1) - 1)
            {
                value += (boidLists[i + 1, j + 1].Count) / neighbourWeight;
            }
            if (i > 0 && j < boidLists.GetLength(1) - 1)
            {
                value += (boidLists[i - 1, j + 1].Count) / neighbourWeight;
            }
            if (i < boidLists.GetLength(0) - 1 && j > 0)
            {
                value += (boidLists[i + 1, j - 1].Count) / neighbourWeight;
            }

            value /= divider;

            return Color.Lerp(Color.LightBlue, Color.Red, value);
        }
    }
}
