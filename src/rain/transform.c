#include <rain/transform.h>

// struct rain_transform *rain_transform_fix(struct rain_transform *this) {
// 	if (this->dirty_) {
// 		this->matrix = HMM_Translate(this->position_);
// 		this->matrix = HMM_Mul(this->matrix, HMM_QToM4(this->rotation_));
// 		this->matrix = HMM_Mul(this->matrix, HMM_Scale(this->scale_));
// 		this->dirty_ = false;
// 	}
// 	return this;
// }
