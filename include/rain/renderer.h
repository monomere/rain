#ifndef RAIN__RENDERER_H_
#define RAIN__RENDERER_H_

#include <rain/compat.h>
#include <stddef.h>
#include <sokol_gfx.h>
#include <rain/window.h>
#include <rain/texture.h>
#include <rain/transform.h>
#include <rain/camera.h>
#include <rain/math.h>

struct rain_renderer {
	struct rain_window *window;
	struct rain__renderer_current_ {
		sg_pipeline pipeline;
		sg_bindings bind;
	} current_;
	struct rain__renderer_builtin_ {
		sg_pipeline textured_quad_pipeline;
		sg_pipeline colored_quad_pipeline;
		sg_shader textured_quad_shader;
		sg_shader colored_quad_shader;
		sg_buffer quad_vertex_buffer;
		sg_sampler nearest_sampler;
	} builtin_;
};

/** initialize renderer. window must be initialized.  */
void rain_renderer_init(
	struct rain_renderer *RAIN_RESTRICT this_,
	struct rain_window *RAIN_RESTRICT window
);

/** deinitialize renderer. */
void rain_renderer_deinit(struct rain_renderer *this_);

/** begin rendering a frame. */
void rain_renderer_begin_render(struct rain_renderer *this_);

/** end rendering a frame. */
void rain_renderer_end_render(struct rain_renderer *this_);

void rain_renderer_render_textured_quad(
	struct rain_renderer *RAIN_RESTRICT this_,
	const struct rain_texture *RAIN_RESTRICT texture,
	const struct sg_sampler sampler,
	size_t offset_x, size_t offset_y, size_t width, size_t height,
	const rain_float4x4 *RAIN_RESTRICT transform
);

void rain_renderer_render_colored_quad(
	struct rain_renderer *RAIN_RESTRICT this_,
	rain_float4 color,
	const rain_float4x4 *RAIN_RESTRICT transform
);

#endif // RAIN__RENDERER_H_
