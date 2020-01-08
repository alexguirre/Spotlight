namespace Spotlight.Core
{
    using Rage;

    using Spotlight.Core.Memory;

    internal unsafe class VehicleBone
    {
        private readonly Vehicle vehicle;
        private readonly int index;
        private readonly fragInst* vehicleInst;

        public Vehicle Vehicle => vehicle;
        public int Index => index;
        public Matrix Transform
        {
            get => vehicleInst->entry->skeleton->desiredBonesTransformsArray[index];
            set => vehicleInst->entry->skeleton->desiredBonesTransformsArray[index] = value;
        }

        public Vector3 Translation
        {
            get => *(NativeVector3*)&vehicleInst->entry->skeleton->desiredBonesTransformsArray[index].M41;
        }

        public Vector3 OriginalTranslation { get; }
        public Quaternion OriginalRotation { get; }

        private VehicleBone(Vehicle vehicle, int index)
        {
            this.vehicle = vehicle;
            this.index = index;

            CVehicle* v = ((CVehicle*)vehicle.MemoryAddress);
            vehicleInst = v->inst;

            OriginalTranslation = Utility.GetBoneOriginalTranslation(vehicle, index);
            OriginalRotation = Utility.GetBoneOriginalRotation(vehicle, index);
        }

        public void RotateAxis(Vector3 axis, float degrees)
        {
            Matrix* matrix = &(vehicleInst->entry->skeleton->desiredBonesTransformsArray[index]);
            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(axis, MathHelper.ConvertDegreesToRadians(degrees)) * (*matrix);
            *matrix = newMatrix;
        }

        public void Translate(Vector3 translation)
        {
            Matrix* matrix = &(vehicleInst->entry->skeleton->desiredBonesTransformsArray[index]);
            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(translation) * (*matrix);
            *matrix = newMatrix;
        }

        public void SetRotation(Quaternion rotation)
        {
            Matrix* matrix = &(vehicleInst->entry->skeleton->desiredBonesTransformsArray[index]);
            Utility.DecomposeMatrix(*matrix, out Vector3 scale, out _, out Vector3 translation);
            Matrix newMatrix = Matrix.Scaling(scale) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);
            *matrix = newMatrix;
        }

        public void SetTranslation(Vector3 translation)
        {
            Matrix* matrix = &(vehicleInst->entry->skeleton->desiredBonesTransformsArray[index]);
            matrix->M41 = translation.X;
            matrix->M42 = translation.Y;
            matrix->M43 = translation.Z;
        }

        public void ResetRotation() => SetRotation(OriginalRotation);
        public void ResetTranslation() => SetTranslation(OriginalTranslation);


        public static bool TryGetForVehicle(Vehicle vehicle, string boneName, out VehicleBone bone)
        {
            int boneIndex = vehicle.GetBoneIndex(boneName);
            return TryGetForVehicle(vehicle, boneIndex, out bone);
        }

        public static bool TryGetForVehicle(Vehicle vehicle, int boneIndex, out VehicleBone bone)
        {
            if (boneIndex == -1)
            {
                bone = null;
                return false;
            }

            bone = new VehicleBone(vehicle, boneIndex);
            return true;
        }
    }
}
