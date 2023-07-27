#ifndef RAIN__UTIL__H_
#define RAIN__UTIL__H_

#define RAIN__SET_DIRTY_FIELD_FUNC_(NAME, THIS, TYPE, FIELD, DIRTY) \
	static inline void NAME##_##FIELD(THIS *this, TYPE FIELD) { \
		this->FIELD##_ = FIELD; this->DIRTY = true; \
	}

#endif // RAIN__UTIL__H_
