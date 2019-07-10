using Lockstep.Math;
using static Lockstep.Math.LMath;
using Point2D = Lockstep.Math.LVector2;

namespace Lockstep.Collision
{
    [System.Serializable]
    public partial class OBB : BaseShape

    {
        /// <summary>
        /// Collision Type
        /// </summary>
        public override EColType ColType
        {
            get { return EColType.OBB; }
        }

        /// <summary>
        /// OBB center point
        /// </summary>
        public LVector3 c;

        /// <summary>
        /// Local x-, y-, and z-axes
        /// </summary>
        public LAxis3D u;

        /// <summary>
        /// Positive halfwidth extents of OBB along each axis
        /// </summary>
        public LVector3 e;
        
        public AABB ToAABB()
        {
            var aabb = new AABB();
            var abse = e.abs;
            aabb.min = c - abse;
            aabb.max = c + abse;
            return aabb;
        }
        
        
        public override Sphere GetBoundSphere()
        {
            return new Sphere(c,e.magnitude);
        }
        
        public override void UpdateRotation(LVector3 forward, LVector3 up){
            u = new LAxis3D(Cross(forward,up),up,forward);
        }

        public override void UpdatePosition(LVector3 targetPos){
            c = targetPos;
        }

        
        public override bool TestWithShape(BaseShape shape)
        {
            return shape.TestWith(this);
        }
        
        public override bool TestWith(Sphere sphere)
        {
            return Utils.TestSphereOBB(sphere, this,out LVector3 p);
        }
        public override bool TestWith(AABB aabb)
        {
            //TODO 改为更加高效的判定方式
            return Utils.TestOBBOBB(this, aabb.ToOBB());
        }
        public override bool TestWith(Capsule capsule)
        {
            return Utils.TestOBBCapsule(this, capsule);
        }

        public override bool TestWith(OBB obb)
        {
            return Utils.TestOBBOBB(obb, this);
        }
        
        public override bool TestWith(Plane plane)
        {
            return Utils.TestOBBPlane(this, plane);
        }

        public override bool TestWith(Ray ray)
        {
            return Utils.IntersectRayOBB(ray.o, ray.d, this, out LFloat tmin, out LVector3 temp);
        }

    };
}