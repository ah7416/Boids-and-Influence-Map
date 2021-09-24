using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids
{
    class Boid
    {

        private Vector2 pos, previousPos, velocity, center, acceleration, textureCenter;
        public Vector2 Center { get { return center; } }
        private float rotation = 0, detectionRange = 100, speed = 0.1f, maxforce = 0.03f, maxspeed = 1.5f;    // Maximum speed;
        private Microsoft.Xna.Framework.Rectangle destRect;
        private Texture2D tex, blankTexture;
        private List<Boid> neighbours;
        private static Random rand = new Random();
        private int windowHeight, windowWidth;
        bool hasNeighBours = false;

        public Boid(Texture2D tex, Texture2D blankTexture, Vector2 pos, int windowWidth, int windowHeight)
        {
            this.tex = tex;
            this.pos = pos;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            this.blankTexture = blankTexture;

            rotation = (float)(rand.NextDouble() * Math.PI * 2);
            velocity = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            acceleration = new Vector2(0, 0);
            destRect = new Microsoft.Xna.Framework.Rectangle((int)pos.X, (int)pos.Y, 20, 20);
            center = new Vector2(pos.X + (destRect.Width / 2), pos.Y + (destRect.Height / 2));
            textureCenter = new Vector2(tex.Width / 2, tex.Height / 2);
        }

        public void Update(GameTime gameTime, Boid[] boidArray, InfluenceMap influenceMap)
        {
            previousPos = pos;
            
            DetectNeighbours(boidArray, influenceMap);
            Flock();

            // Update velocity
            velocity += acceleration;
            // Limit speed
            velocity = Limit(velocity, maxspeed);
            //Update position
            pos += velocity * speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            // Reset accelertion to 0 each cycle
            acceleration *= 0;
            //Update center and destination rectangle
            center.X = pos.X + (destRect.Width / 2);
            center.Y = pos.Y + (destRect.Height / 2);
            destRect.X = (int)center.X;
            destRect.Y = (int)center.Y;

            rotation = GetRotationAngleInRadians();
            WrapAround();
        }

        //Enable exiting one side of the screen and entering the opposite side
        private void WrapAround()
        {
            if (pos.X < -destRect.Width) pos.X = windowWidth + destRect.Width;
            if (pos.Y < -destRect.Height) pos.Y = windowHeight + destRect.Height;
            if (pos.X > windowWidth + destRect.Width) pos.X = -destRect.Width;
            if (pos.Y > windowHeight + destRect.Height) pos.Y = -destRect.Height;
        }

        // We accumulate a new acceleration each time based on three rules
        private void Flock()    
        {
            Vector2 sep = Separate();   // Separation
            Vector2 ali = Align();      // Alignment
            Vector2 coh = Cohesion();   // Cohesion
           
            // Arbitrarily weight these forces
            sep *= 1.5f;
            ali *= 1.0f;
            coh *= 1.0f;

            // Add the force vectors to acceleration
            ApplyForce(sep);
            ApplyForce(ali);
            ApplyForce(coh);
        }

        // Separation
        // Method checks for nearby boids and return a force vector for steering away
        Vector2 Separate()
        {
            float desiredseparation = 90.0f;
            Vector2 steer = new Vector2(0, 0);
            int count = 0;
            // For every boid in neighbours, check if it's too close
            foreach (Boid other in neighbours)
            {
                float d = Vector2.Distance(center, other.Center);
                // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                if ((d > 0) && (d < desiredseparation))
                {
                    // Calculate vector pointing away from neighbor
                    Vector2 diff = center - other.Center;
                    diff.Normalize();
                    diff /= d;        // Weight by distance
                    steer += diff;
                    count++;            // Keep track of how many
                }
            }
            // Average -- divide by how many we are to close to
            if (count > 0)
            {
                steer /= ((float)count);
            }

            // As long as the vector is greater than 0
            if (steer.Length() > 0)
            {
                //Steering = Desired - Velocity
                steer.Normalize();
                steer *= maxspeed;
                steer -= velocity;
                steer = Limit(steer, maxforce);
            }
            return steer;

        }

        private Vector2 Limit(Vector2 target, float maxLength)
        {
            float lengthSquared = target.X * target.X + target.Y * target.Y;

            if ((lengthSquared > maxLength * maxLength) && (lengthSquared > 0))
            {
                float ratio = (float)(maxLength / Math.Sqrt(lengthSquared));
                target.X *= ratio;
                target.Y *= ratio;
            }

            return target;
        }

        // Alignment
        // For every nearby boid in the system, calculate the average velocity
        Vector2 Align()
        {
            float neighbordist = detectionRange;
            Vector2 sum = new Vector2(0, 0);
            int count = 0;
            foreach (Boid other in neighbours)
            {
                float d = Vector2.Distance(center, other.Center);
                if ((d > 0) && (d < neighbordist))
                {
                    sum += other.velocity;
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= ((float)count);

                sum.Normalize();
                sum *= (maxspeed);
                Vector2 steer = sum - velocity;
                steer = Limit(steer, maxforce);
                return steer;
            }
            else
            {
                return new Vector2(0, 0);
            }
        }

        // Cohesion
        // For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
        Vector2 Cohesion()
        {
            float neighbordist = detectionRange;
            Vector2 sum = new Vector2(0, 0);   // Start with empty vector to accumulate all positions
            int count = 0;
            foreach (Boid other in neighbours)
            {
                float d = Vector2.Distance(center, other.Center);
                if ((d > 0) && (d < neighbordist))
                {
                    sum += other.Center; // Add position
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= count;
                return Seek(sum);  // Steer towards the position
            }
            else
            {
                return new Vector2(0, 0);
            }
        }

        // Calculates and returns a steering force towards a target
        Vector2 Seek(Vector2 target)
        {
            Vector2 desired = target - center;  // A vector pointing from the position to the target
                                                // Scale to maximum speed
            desired.Normalize();
            desired *= maxspeed;

            // Steering = Desired minus Velocity
            Vector2 steer = desired - velocity;
            steer = Limit(steer, maxforce);  // Limit to maximum steering force
            return steer;
        }

        //Add upp forces
        private void ApplyForce(Vector2 force)
        {
            acceleration += force;
        }

        //Detect which of all boids are visible
        private void DetectNeighbours(Boid[] boidArray, InfluenceMap influenceMap)
        {
            List<Boid> tempBoidList = new List<Boid>();
            hasNeighBours = false;
            neighbours = new List<Boid>();
            int cellsToInclude = (int)detectionRange / influenceMap.tileSize;
            List<Tuple<int, int>> tilesToCheck = influenceMap.GetSurroundingTiles((int)center.X / influenceMap.tileSize, (int)center.Y / influenceMap.tileSize, cellsToInclude);

            foreach (Tuple<int,int> t in tilesToCheck)
            {
                tempBoidList.AddRange(influenceMap.GetBoidsInCell(t.Item1, t.Item2));
            }

            for (int i = 0; i < Game1.numberOfBoids; i++)
            {
                Boid other = boidArray[i];
                if (!other.Equals(this))
                {
                    float dist = Vector2.Distance(center, other.Center);
                    if (dist < detectionRange)
                    {
                        if (!isInsideSector(other, center, detectionRange, rotation + MathHelper.ToRadians(150), rotation + MathHelper.ToRadians(210)))
                        {
                            neighbours.Add(other);
                            hasNeighBours = true;
                        }
                    }
                }
            }
        }

        //Check if boid is within circlesector
        bool isInsideSector(Boid other, Vector2 center, float radius, float angle1, float angle2)
        {
            Vector2 relVec = new Vector2(other.Center.X - center.X, other.Center.Y - center.Y);

            return !AreClockwise(center, radius, angle1, relVec) &&
                   AreClockwise(center, radius, angle2, relVec);

        }

        
        bool AreClockwise(Vector2 center, float radius, float angle, Vector2 otherCenter)
        {
            Vector2 point1 = new Vector2((center.X + radius) * (float)Math.Cos(angle), (center.Y + radius) * (float)Math.Sin(angle));
            return -point1.X * otherCenter.Y + point1.Y * otherCenter.X > 0;
        }

        private float ToRadians(float degrees)
        {
            return (degrees * (float)(Math.PI / 180));
        }

        protected float GetRotationAngleInRadians()
        {
            Vector2 travelledVector = pos - previousPos;
            float radians;
            travelledVector.Normalize();
             radians = (float)Math.Atan2(travelledVector.Y, travelledVector.X);

            return radians;
        }

        public static Vector2 GetNormalizedDirection(Vector2 pos, Vector2 targetPos)
        {
            Vector2 direction = targetPos - pos;
            direction.Normalize();
            return direction;
        }

        private Vector2 GetVecFromRadians(float radians)
        {
            Vector2 directionVector = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            return directionVector;
        }

        public void Draw(SpriteBatch sb)
        {
            if (Game1.debug)
            {
                float count = 0;
                foreach (Boid boid in neighbours)
                {
                    count++;
                }

                //Draw field of sight
                DrawingExtensions.DrawCone(sb, center, detectionRange, Microsoft.Xna.Framework.Color.Green * 0.5f, 2, ToRadians(300), rotation, blankTexture);

                if (hasNeighBours)
                {

                    sb.Draw(tex, destRect, null, Microsoft.Xna.Framework.Color.Red * (count / 10.0f), rotation, textureCenter, SpriteEffects.None, 0);
                }
                else
                {
                    sb.Draw(tex, destRect, null, Microsoft.Xna.Framework.Color.White, rotation, textureCenter, SpriteEffects.None, 0);

                }
            }
            else
            {
                sb.Draw(tex, destRect, null, Microsoft.Xna.Framework.Color.White, rotation, textureCenter, SpriteEffects.None, 0);
            }   
        }
    }
}
