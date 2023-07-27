#ifndef RAIN__CAMERA_H_
#define RAIN__CAMERA_H_

/** @deprecated */

// #include <rain/util_.h>
// #include <rain/math.h>

// enum rain_camera_projection {
// 	RAIN_CAMERA_PROJECTION_PERSPECTIVE,
// 	RAIN_CAMERA_PROJECTION_ORTHOGRAPHIC,
// };

// struct rain_camera {
// 	bool dirty_view_, dirty_proj_;
	
// 	rain_float3 position_;
// 	rain_float4 rotation_;
// 	rain_float4x4 view_matrix;

// 	enum rain_camera_projection type_;
// 	union {
// 		float fov_;
// 		float vert_size_;
// 	};
// 	float aspect_, near_, far_;
// 	HMM_Mat4 proj_matrix;
// };

// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, HMM_Vec3, position, dirty_view_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, HMM_Quat, rotation, dirty_view_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, float, fov, dirty_proj_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, float, aspect, dirty_proj_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, float, near, dirty_proj_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, float, far, dirty_proj_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera, float, vert_size, dirty_proj_);
// RAIN__SET_DIRTY_FIELD_FUNC_(rain_camera_set, struct rain_camera,
// 	enum rain_camera_projection, type, dirty_proj_);

// struct rain_camera *rain_camera_fix(struct rain_camera *this);
#endif // RAIN__CAMERA_H_
