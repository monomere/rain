#include <rain/camera.h>
#include <stdio.h>

// struct rain_camera *rain_camera_fix(struct rain_camera *this) {
// 	if (this->dirty_view_) {
// 		this->view_matrix = HMM_M4D(1.0f);
// 		// this->view_matrix = HMM_Mul(this->view_matrix, HMM_Translate(HMM_MulV3F(this->position_, -1.0f)));
// 		this->dirty_view_ = false;
// 	}
// 	if (this->dirty_proj_) {
// 		if (this->type_ == RAIN_CAMERA_PROJECTION_ORTHOGRAPHIC) {
// 			float horiz_size = this->vert_size_ * this->aspect_;
// 			this->proj_matrix = HMM_Orthographic_RH_NO(
// 				-horiz_size / 2, +horiz_size / 2,
// 				-this->vert_size_ / 2, +this->vert_size_ / 2,
// 				this->near_, this->far_
// 			);
// 		} else if (this->type_ == RAIN_CAMERA_PROJECTION_PERSPECTIVE) {
// 			this->proj_matrix = HMM_Perspective_RH_NO(this->fov_, this->aspect_, this->near_, this->far_);
// 		}
// 		this->dirty_proj_ = false;
// 	}
// 	return this;
// }
