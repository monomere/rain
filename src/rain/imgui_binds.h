#ifndef RAIN__IMGUI_BINDS_H_
#define RAIN__IMGUI_BINDS_H_

#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

void rain_imgui_init(size_t max_vertices);
void rain_imgui_deinit();
void rain_imgui_begin_render();
void rain_imgui_end_render();
bool rain_imgui_begin(const char *name, bool *is_open);
void rain_imgui_end();

#ifdef __cplusplus
} // extern "C"
#endif

#endif // RAIN__IMGUI_BINDS_H_
