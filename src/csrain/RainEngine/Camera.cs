using System;
using System.Numerics;

namespace RainEngine
{
	public class Camera
	{
		public enum ProjectionType { Perspective, Orthographic }

		private bool _ViewDirty, _ProjDirty;
		private Matrix4x4 _ViewMatrixCache, _ProjMatrixCache;

		public Matrix4x4 ComputeTransformMatrix(Matrix4x4 model)
		{
			return ProjMatrix * ViewMatrix * Matrix4x4.Transpose(model);
		}

		public Matrix4x4 ViewMatrix
		{
			get
			{
				if (_ViewDirty)
				{
					_ViewMatrixCache
						= Matrix4x4.CreateTranslation(-Position)
						* Matrix4x4.CreateFromQuaternion(Rotation);
					_ViewDirty = false;
				}
				return _ViewMatrixCache;
			}
		}

		public Matrix4x4 ProjMatrix
		{
			get
			{
				if (_ProjDirty)
				{
					if (Type == ProjectionType.Perspective)
					{
						_ProjMatrixCache = Matrix4x4.CreatePerspectiveFieldOfView(Fov, Aspect, Near, Far);
					}
					if (Type == ProjectionType.Orthographic)
					{
						_ProjMatrixCache = Matrix4x4.CreateOrthographic(
							_Aspect * _VerticalSize,
							_VerticalSize,
							_Near, _Far
						);
					}
					_ProjDirty = false;
				}
				return _ProjMatrixCache;
			}
		}

		private Camera(ProjectionType type, float fov, float verticalSize, float aspect, float near, float far)
		{
			this.Type = type;
			this.Fov = fov;
			this.VerticalSize = verticalSize;
			this.Aspect = aspect;
			this.Near = near;
			this.Far = far;
		}

		private ProjectionType _Type;
		private float _Fov;
		private float _VerticalSize;
		private float _Aspect;
		private float _Near;
		private float _Far;

		public ProjectionType Type { get => _Type; set { _Type = value; _ProjDirty = true; } }
		public float Fov { get => _Fov; set { _Fov = value; _ProjDirty = true; } }
		public float VerticalSize { get => _VerticalSize; set { _VerticalSize = value; _ProjDirty = true; } }
		public float Aspect { get => _Aspect; set { _Aspect = value; _ProjDirty = true; } }
		public float Near { get => _Near; set { _Near = value; _ProjDirty = true; } }
		public float Far { get => _Far; set { _Far = value; _ProjDirty = true; } }

		private Vector3 _Position;
		private Quaternion _Rotation;
		public Vector3 Position { get => _Position; set { _Position = value; _ViewDirty = true; } }
		public Quaternion Rotation { get => _Rotation; set { _Rotation = value; _ViewDirty = true; } }

		public static Camera Orthographic(float verticalSize, float near = 0.01f, float far = 10.0f)
		{
			return new(ProjectionType.Orthographic, 0.0f, verticalSize, Window.Active.AspectRatio, near, far);
		}

		public static Camera Perspective(float fov, float near = 0.01f, float far = 10.0f)
		{
			return new(ProjectionType.Orthographic, fov, 0.0f, Window.Active.AspectRatio, near, far);
		}

		public static Camera Active => Scene.Active.ActiveCamera;
	}
}
