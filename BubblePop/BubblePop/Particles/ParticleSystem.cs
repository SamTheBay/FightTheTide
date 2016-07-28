using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BubblePop
{
    public enum ParticleType
    {
        Explosion,
        ExplosionSmoke,
        SmokePlume,
        Starburst
    }

    public static class ParticleSystem
    {
        // the array of particles used by this system. these are reused, so that calling
        // AddParticles will not cause any allocations.
        private static Particle[] particles;

        // the queue of free particles keeps track of particles that are not curently
        // being used by an effect. when a new effect is requested, particles are taken
        // from this queue. when particles are finished they are put onto this queue.
        private static Queue<Particle> freeParticles;

        private static ParticleParams explosionParticleParams = new ParticleParams()
        {
            TextureName = "ExplosionParticle",

            // high initial speed with lots of variance.  make the values closer
            // together to have more consistently circular explosions.
            MinInitialSpeed = 10,
            MaxInitialSpeed = 100,

            // doesn't matter what these values are set to, acceleration is tweaked in
            // the override of InitializeParticle.
            MinAcceleration = 0,
            MaxAcceleration = 0,

            // explosions should be relatively short lived
            MinLifetime = .5f,
            MaxLifetime = 1.0f,

            MinScale = .1f,
            MaxScale = .3f,

            MinNumParticles = 5,
            MaxNumParticles = 12,

            MinRotationSpeed = -MathHelper.PiOver4,
            MaxRotationSpeed = MathHelper.PiOver4,

            Blend = BlendState.Additive,

            PostInit = (p) => p.Acceleration = -p.Velocity / p.Lifetime
        };

        private static Dictionary<ParticleType, ParticleParams> typeToParams = new Dictionary<ParticleType, ParticleParams>()
        {
            {
                ParticleType.Explosion,
                explosionParticleParams
            },
            {
                ParticleType.ExplosionSmoke,
                new ParticleParams()
                {
                    TextureName = "SmokeParticle",

                    // less initial speed than the explosion itself
                    MinInitialSpeed = 20,
                    MaxInitialSpeed = 200,

                    // acceleration is negative, so particles will accelerate away from the
                    // initial velocity.  this will make them slow down, as if from wind
                    // resistance. we want the smoke to linger a bit and feel wispy, though,
                    // so we don't stop them completely like we do ExplosionParticleSystem
                    // particles.
                    MinAcceleration = -10,
                    MaxAcceleration = -50,

                    // explosion smoke lasts for longer than the explosion itself, but not
                    // as long as the plumes do.
                    MinLifetime = 1.0f,
                    MaxLifetime = 2.5f,

                    MinScale = 1.0f,
                    MaxScale = 2.0f,

                    // we need to reduce the number of particles on Windows Phone in order to keep
                    // a good framerate
                    MinNumParticles = 5,
                    MaxNumParticles = 10,

                    MinRotationSpeed = -MathHelper.PiOver4,
                    MaxRotationSpeed = MathHelper.PiOver4
                }
            },
            {
                ParticleType.SmokePlume,
                new ParticleParams()
                {
                    TextureName = "SmokeParticle",

                    MinInitialSpeed = 20,
                    MaxInitialSpeed = 100,

                    // we don't want the particles to accelerate at all, aside from what we
                    // do in our overriden InitializeParticle.
                    MinAcceleration = 0,
                    MaxAcceleration = 0,

                    // long lifetime, this can be changed to create thinner or thicker smoke.
                    // tweak minNumParticles and maxNumParticles to complement the effect.
                    MinLifetime = 5.0f,
                    MaxLifetime = 7.0f,

                    MinScale = .5f,
                    MaxScale = 1.0f,

                    // we need to reduce the number of particles on Windows Phone in order to keep
                    // a good framerate
                    MinNumParticles = 3,
                    MaxNumParticles = 8,

                    // rotate slowly, we want a fairly relaxed effect
                    MinRotationSpeed = -MathHelper.PiOver4 / 2.0f,
                    MaxRotationSpeed = MathHelper.PiOver4 / 2.0f,

                    PickRandomDirection = () =>
                    {
                        // Point the particles up, somewhere between 80 and 100 degrees.
                        // tweak this to make the smoke have more or less spread.
                        float radians = Util.RandomBetween(
                            MathHelper.ToRadians(120), MathHelper.ToRadians(100));

                        Vector2 direction = Vector2.Zero;
                        // from the unit circle, cosine is the x coordinate and sine is the
                        // y coordinate. We're negating y because on the screen increasing y moves
                        // down the monitor.
                        direction.X = (float)Math.Cos(radians);
                        direction.Y = -(float)Math.Sin(radians);
                        return direction;
                    },

                    PostInit = (Particle p) =>
                    {
                        // the base is mostly good, but we want to simulate a little bit of wind
                        // heading to the right.
                        p.Acceleration.X -= Util.RandomBetween(10, 50);
                    }
                }
            },
            {
                ParticleType.Starburst,
                new ParticleParams()
                {
                    TextureName = "StarburstParticle",

                    // high initial speed with lots of variance.  make the values closer
                    // together to have more consistently circular explosions.
                    MinInitialSpeed = 50,
                    MaxInitialSpeed = 100,

                    // doesn't matter what these values are set to, acceleration is tweaked in
                    // the override of InitializeParticle.
                    MinAcceleration = 0,
                    MaxAcceleration = 0,

                    // explosions should be relatively short lived
                    MinLifetime = .5f,
                    MaxLifetime = 1.0f,

                    MinScale = .05f,
                    MaxScale = .2f,

                    MinNumParticles = 15,
                    MaxNumParticles = 20,

                    MinRotationSpeed = -MathHelper.PiOver4,
                    MaxRotationSpeed = MathHelper.PiOver4,

                    Blend = BlendState.Additive,

                    PostInit = (p) => p.Acceleration = -p.Velocity / p.Lifetime
                }
            }
        };

        public static void Initialize(int maxParticles)
        {
            particles = new Particle[maxParticles];
            freeParticles = new Queue<Particle>(maxParticles);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle();
                freeParticles.Enqueue(particles[i]);
            }

            foreach (var param in typeToParams.Values)
            {
                // load the texture....
                param.Texture = InternalContentManager.GetTexture(param.TextureName);

                // ... and calculate the center. this'll be used in the draw call, we
                // always want to rotate and scale around this point.
                param.Origin.X = param.Texture.Width / 2;
                param.Origin.Y = param.Texture.Height / 2;
            }
        }

        public static void AddParticles(Vector2 where, ParticleType particleType, 
            Color? color = null, float sizeScale = 1.0f, float numParticlesScale = 1.0f, float lifetimeScale = 1.0f)
        {
            ParticleParams param = typeToParams[particleType];
            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles = Math.Max(1, (int)Util.RandomBetween(
                param.MinNumParticles * numParticlesScale,
                param.MaxNumParticles * numParticlesScale));

            // create that many particles, if you can.
            for (int i = 0; i < numParticles && freeParticles.Count > 0; i++)
            {
                // grab a particle from the freeParticles queue, and Initialize it.
                Particle p = freeParticles.Dequeue();

                // first, call PickRandomDirection to figure out which way the particle
                // will be moving. velocity and acceleration's values will come from this.
                Vector2 direction = param.PickRandomDirection();

                // pick some random values for our particle
                float velocity =
                    Util.RandomBetween(param.MinInitialSpeed, param.MaxInitialSpeed);
                float acceleration =
                    Util.RandomBetween(param.MinAcceleration, param.MaxAcceleration);
                float lifetime =
                    Util.RandomBetween(param.MinLifetime * lifetimeScale, param.MaxLifetime * lifetimeScale);
                float scale =
                    Util.RandomBetween(
                    param.MinScale * sizeScale,
                    param.MaxScale * sizeScale);
                float rotationSpeed =
                    Util.RandomBetween(param.MinRotationSpeed, param.MaxRotationSpeed);

                // then initialize it with those random values. initialize will save those,
                // and make sure it is marked as active.
                p.Initialize(
                    where, velocity * direction, acceleration * direction,
                    lifetime, scale, rotationSpeed);

                p.TintColor = color.HasValue ? color.Value : param.Color;
                p.Origin = param.Origin;
                p.Texture = param.Texture;
                p.Blend = param.Blend;
            }
        }

        /// <summary>
        /// overriden from DrawableGameComponent, Update will update all of the active
        /// particles.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            // calculate dt, the change in the since the last frame. the particle
            // updates will use this value.
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // go through all of the particles...
            foreach (Particle p in particles)
            {
                if (p.Active)
                {
                    // ... and if they're active, update them.
                    p.Update(dt);
                    // if that update finishes them, put them onto the free particles
                    // queue.
                    if (!p.Active)
                    {
                        freeParticles.Enqueue(p);
                    }
                }
            }
        }

        /// <summary>
        /// overriden from DrawableGameComponent, Draw will use Util's 
        /// sprite batch to render all of the active particles.
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Draw(spriteBatch, BlendState.AlphaBlend, gameTime);
            Draw(spriteBatch, BlendState.Additive, gameTime);
            Draw(spriteBatch, BlendState.Opaque, gameTime);
        }

        private static void Draw(SpriteBatch spriteBatch, BlendState blend, GameTime gameTime)
        {
            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
            spriteBatch.Begin(SpriteSortMode.Deferred, blend);

            foreach (Particle p in particles)
            {
                // skip inactive particles
                if (!p.Active)
                    continue;

                // skip particles that aren't in this blend state
                if (p.Blend != blend)
                    continue;

                // normalized lifetime is a value from 0 to 1 and represents how far
                // a particle is through its life. 0 means it just started, .5 is half
                // way through, and 1.0 means it's just about to be finished.
                // this value will be used to calculate alpha and scale, to avoid 
                // having particles suddenly appear or disappear.
                float normalizedLifetime = p.TimeSinceStart / p.Lifetime;

                // we want particles to fade in and fade out, so we'll calculate alpha
                // to be (normalizedLifetime) * (1-normalizedLifetime). this way, when
                // normalizedLifetime is 0 or 1, alpha is 0. the maximum value is at
                // normalizedLifetime = .5, and is
                // (normalizedLifetime) * (1-normalizedLifetime)
                // (.5)                 * (1-.5)
                // .25
                // since we want the maximum alpha to be 1, not .25, we'll scale the 
                // entire equation by 4.
                float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
                Color color = p.TintColor * alpha;

                // make particles grow as they age. they'll start at 75% of their size,
                // and increase to 100% once they're finished.
                float scale = p.Scale * (.75f + .25f * normalizedLifetime);

                spriteBatch.Draw(p.Texture, p.Position, null, color,
                    p.Rotation, p.Origin, scale, SpriteEffects.None, 0.0f);
            }

            spriteBatch.End();
        }
    }
}
