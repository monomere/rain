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
bool rain_imgui_begin(const char *name, bool *is_open);
bool rain_imgui_button(const char *label);
void rain_imgui_label(const char *text);
void rain_imgui_image(const struct rain_texture *texture, float width, float height);
void rain_imgui_end();
void rain_imgui_demo();

void rain_imgui_drag_float(const char *l, float *v);
void rain_imgui_drag_float2(const char *l, rain_float2 *v);
void rain_imgui_drag_float3(const char *l, rain_float3 *v);
void rain_imgui_drag_float4(const char *l, rain_float4 *v);
void rain_imgui_input_float(const char *l, float *v);
void rain_imgui_input_float2(const char *l, rain_float2 *v);
void rain_imgui_input_float3(const char *l, rain_float3 *v);
void rain_imgui_input_float4(const char *l, rain_float4 *v);
void rain_imgui_slider_float(const char *l, float *v, float min, float max);
void rain_imgui_slider_float2(const char *l, rain_float2 *v, float min, float max);
void rain_imgui_slider_float3(const char *l, rain_float3 *v, float min, float max);
void rain_imgui_slider_float4(const char *l, rain_float4 *v, float min, float max);
void rain_imgui_input_text(const char *l, char *buf, size_t size);
bool rain_imgui_is_item_clicked();
bool rain_imgui_tree_node(void *id, bool *selected, const char *label);
void rain_imgui_tree_pop();

#ifdef __cplusplus
} // extern "C"
#endif

#endif // RAIN__IMGUI_BINDS_H_
