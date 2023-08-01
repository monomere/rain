#ifndef RAIN__IMGUI_BINDS_H_
#define RAIN__IMGUI_BINDS_H_

#include <stddef.h>
#include <rain/math.h>

#ifdef __cplusplus
extern "C" {
#endif

struct rain_texture;

struct rain_imgui_data {
	void *context;
	void *free_func, *alloc_func;
	void *user_ptr;
};

void rain_imgui_init(size_t max_vertices, struct rain_imgui_data *data);
void rain_imgui_deinit();
void rain_imgui_begin_render();
void rain_imgui_end_render();

#ifdef __cplusplus
} // extern "C"
#endif

#endif // RAIN__IMGUI_BINDS_H_
