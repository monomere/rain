#include <stdio.h>

#include <rain/renderer.h>

#include <GL/gl3w.h>
#include "glfw.h"

enum rain__uniform_block_index_ {
	RAIN__UNIFORM_BLOCK_INDEX_GLOBAL_ = 0,
	RAIN__UNIFORM_BLOCK_INDEX_FRAME_ = 1,
	RAIN__UNIFORM_BLOCK_INDEX_MATERIAL_ = 2,
	RAIN__UNIFORM_BLOCK_INDEX_INSTANCE_ = 3,
};

void logger_for_sg(
	const char *tag,      /* always "sg" */
	uint32_t log_level,   /* 0=panic, 1=error, 2=warning, 3=info */
	uint32_t log_item_id, /* SG_LOGITEM_* */
	const char *message,  /* a message string, may be nullptr in release mode */
	uint32_t lineno,      /* line number in sokol_gfx.h */
	const char *filename, /* source filename, may be nullptr in release mode */
	void* user_data
) {
	if (log_level > 3) return;
	if (message == nullptr) return;
	static const char *log_level_strings[4] = { "PANIC", "ERROR", "WARN", "INFO" };
	const char *log_level_string = log_level_strings[log_level];

	fprintf(stderr, "%s/%s %s (sokol_gfx.h:%d)\n", tag, log_level_string, message, lineno);
}

struct rain__ub_data_quad_ {
	rain_float4x4 trans;
};

struct rain__ub_data_colored_quad_ {
	struct rain__ub_data_colored_quad_vs_ {
		struct rain__ub_data_quad_ quad;
	} vs;
	struct rain__ub_data_colored_quad_fs_ {
		rain_float4 color;
	} fs;
};

struct rain__ub_data_textured_quad_ {
	struct rain__ub_data_textured_quad_vs_ {
		struct rain__ub_data_quad_ quad;
		rain_float4 uvs;
	} vs;
	struct rain__ub_data_textured_quad_fs_ {
		rain_float4 tint;
	} fs;
	struct rain__ub_data_textured_quad_bind_ {
		uint32_t texture;
	} bind;
};

static inline void rain___renderer_bind_pipeline(struct rain_renderer *this, sg_pipeline pipeline) {
	if (this->current_.pipeline.id != pipeline.id) {
		this->current_.pipeline = pipeline;
		sg_apply_pipeline(this->current_.pipeline);
	}
}

static inline void rain___renderer_bind_vertex_buffer(struct rain_renderer *this, sg_buffer buffer) {
	this->current_.bind.vertex_buffers[0] = buffer;
}

void rain_renderer_init(
	struct rain_renderer *restrict this,
	struct rain_window *restrict window
) {
	this->window = window;

	sg_setup(&(sg_desc) {
		.context = rain_window_get_context(this->window),
		.logger.func = &logger_for_sg,
	});

	this->builtin_.colored_quad_shader = sg_make_shader(&(sg_shader_desc){
		.label = "Builtin Colored Quad Shader",
		.vs = {
			.uniform_blocks[0] = {
				.size = sizeof(struct rain__ub_data_colored_quad_vs_),
				.uniforms = {
					[0] = { .name = "u_trans", .type = SG_UNIFORMTYPE_MAT4 },
				},
			},
			.source = 
				"#version 330\n"
				"layout(location = 0) in vec2 i_position;\n"
				"uniform mat4 u_trans;\n"
				"void main() {\n"
				"  gl_Position = vec4(i_position, 0.0, 1.0) * u_trans;\n"
				"}\n",
		},
		.fs = {
			.uniform_blocks[0] = {
				.size = sizeof(struct rain__ub_data_colored_quad_fs_),
				.uniforms = {
					[0] = { .name = "u_color", .type = SG_UNIFORMTYPE_FLOAT4 },
				},
			},
			.source = 
				"#version 330\n"
				"uniform vec4 u_color;\n"
				"in vec2 s_uv;"
				"out vec4 o_color;"
				"void main() {\n"
				"  o_color = u_color;"
				"}\n",
		}
	});

	sg_blend_state blending = {
		.enabled = true,
		.src_factor_rgb = SG_BLENDFACTOR_SRC_ALPHA,
		.dst_factor_rgb = SG_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
		.op_rgb = SG_BLENDOP_ADD,
		.src_factor_alpha = SG_BLENDFACTOR_ZERO,
		.dst_factor_alpha = SG_BLENDFACTOR_ONE,
		.op_alpha = SG_BLENDOP_ADD,
	};

	this->builtin_.colored_quad_pipeline = sg_make_pipeline(&(sg_pipeline_desc){
		.label = "Colored Quad Pipeline",
		.layout.attrs[0].format = SG_VERTEXFORMAT_FLOAT2,
		.shader = this->builtin_.colored_quad_shader,
		.index_type = SG_INDEXTYPE_NONE,
		.cull_mode = SG_CULLMODE_NONE,
		.primitive_type = SG_PRIMITIVETYPE_TRIANGLE_STRIP,
		.depth.pixel_format = SG_PIXELFORMAT_DEPTH_STENCIL,
		.colors[0].blend = blending
	});

	this->builtin_.textured_quad_shader = sg_make_shader(&(sg_shader_desc){
		.label = "Builtin Textured Quad Shader",
		.vs = {
			.uniform_blocks[0] = {
				.size = sizeof(struct rain__ub_data_textured_quad_vs_),
				.uniforms = {
					[0] = { .name = "u_trans", .type = SG_UNIFORMTYPE_MAT4 },
					[1] = { .name = "u_uvs", .type = SG_UNIFORMTYPE_FLOAT4 },
				},
			},
			.source = 
				"#version 330\n"
				"layout(location = 0) in vec2 i_position;\n"
				"uniform mat4 u_trans;\n"
				"uniform vec4 u_uvs;\n"
				"out vec2 s_uv;\n"
				"void main() {\n"
				"  vec2[] texcoords = vec2[](\n"
				"    u_uvs.xy, vec2(u_uvs.z, u_uvs.y),\n"
				"    vec2(u_uvs.x, u_uvs.w), u_uvs.zw \n"
				"  );\n"
				"  gl_Position = vec4(i_position, 0.0, 1.0) * u_trans;\n"
				"  s_uv = texcoords[gl_VertexID];\n"
				"}\n",
		},
		.fs = {
			.images[0].used = true,
			.samplers[0].used = true,
			.image_sampler_pairs[0] = {
				.used = true,
				.glsl_name = "u_texture",
				.image_slot = 0,
				.sampler_slot = 0
			},
			.uniform_blocks[0] = {
				.size = sizeof(struct rain__ub_data_textured_quad_fs_),
				.uniforms = {
					[0] = { .name = "u_tint", .type = SG_UNIFORMTYPE_FLOAT4 },
				},
			},
			.source = 
				"#version 330\n"
				"uniform sampler2D u_texture;\n"
				"uniform vec4 u_tint;\n"
				"in vec2 s_uv;"
				"out vec4 o_color;"
				"void main() {\n"
				"  o_color = u_tint * texture(u_texture, s_uv);"
				"}\n",
		}
	});

	this->builtin_.textured_quad_pipeline = sg_make_pipeline(&(sg_pipeline_desc){
		.label = "Textured Quad Pipeline",
		.layout.attrs[0].format = SG_VERTEXFORMAT_FLOAT2,
		.shader = this->builtin_.textured_quad_shader,
		.index_type = SG_INDEXTYPE_NONE,
		.cull_mode = SG_CULLMODE_NONE,
		.primitive_type = SG_PRIMITIVETYPE_TRIANGLE_STRIP,
		.depth.pixel_format = SG_PIXELFORMAT_DEPTH_STENCIL,
		.colors[0].blend = blending
	});

	this->builtin_.quad_vertex_buffer = sg_make_buffer(&(sg_buffer_desc){
		.label = "Quad Vertex Buffer",
		.type = SG_BUFFERTYPE_VERTEXBUFFER,
		.data = (void*)(float[]){
			-1.0f, -1.0f, +1.0f, -1.0f,
			-1.0f, +1.0f, +1.0f, +1.0f,
		},
		.size = 4 * 2 * sizeof(float),
		.usage = SG_USAGE_IMMUTABLE,
	});

	this->builtin_.nearest_sampler = sg_make_sampler(&(sg_sampler_desc){
		.min_filter = SG_FILTER_NEAREST,
    .mag_filter = SG_FILTER_NEAREST,
	});
}

void rain_renderer_deinit(struct rain_renderer *this) {
	sg_destroy_sampler(this->builtin_.nearest_sampler);
	sg_destroy_buffer(this->builtin_.quad_vertex_buffer);
	sg_destroy_pipeline(this->builtin_.colored_quad_pipeline);
	sg_destroy_pipeline(this->builtin_.textured_quad_pipeline);
	sg_destroy_shader(this->builtin_.colored_quad_shader);
	sg_destroy_shader(this->builtin_.textured_quad_shader);
	sg_shutdown();
}

void rain_renderer_begin_render(struct rain_renderer *this) {
	// glPolygonMode(GL_FRONT_AND_BACK, GL_LINE); no support in sokol :c
	this->current_.bind = (sg_bindings){0};
	this->current_.pipeline.id = SG_INVALID_ID;
}

void rain_renderer_end_render(struct rain_renderer *this) {
	sg_commit();
}

// void rain__renderer_compute_trans_matrix_(
// 	struct rain_renderer *restrict this,
// 	HMM_Mat4 *restrict out_mat,
// 	const HMM_Mat4 *restrict transform
// ) {
// 	rain_camera_fix(this->camera);
// 	*out_mat = HMM_Mul(
// 		this->camera->proj_matrix,
// 		HMM_Mul(
// 			this->camera->view_matrix,
// 			HMM_Transpose(*transform)
// 		)
// 	);
// }

void rain_renderer_render_textured_quad(
	struct rain_renderer *restrict this,
	const struct rain_texture *restrict texture,
	const struct sg_sampler sampler,
	const struct rain_renderer_rect *restrict rect,
	const rain_float4 *restrict tint,
	const rain_float4x4 *restrict transform
) {
	struct rain_renderer_rect r = *rect;
	if (r.width == 0) r.width = texture->width;
	if (r.height == 0) r.height = texture->height;

	struct rain__ub_data_textured_quad_ info = {
		.vs.quad.trans = *transform,
		.vs.uvs = {
			.x = r.offset_x /(float) texture->width,
			.y = r.offset_y /(float) texture->height,
			.z = (r.width + r.offset_x) /(float) texture->width,
			.w = (r.height + r.offset_y) /(float) texture->height,
		},
		.fs.tint = {
			tint->x, tint->y, tint->z, tint->w
		}
	};

	rain___renderer_bind_pipeline(this, this->builtin_.textured_quad_pipeline);
	rain___renderer_bind_vertex_buffer(this, this->builtin_.quad_vertex_buffer);

	this->current_.bind.fs.images[0] = texture->image;
	this->current_.bind.fs.samplers[0] = sampler;

	sg_apply_bindings(&this->current_.bind);

	sg_apply_uniforms(
		SG_SHADERSTAGE_VS,
		0,
		&(sg_range){
			.ptr = (uint8_t*)&info + offsetof(struct rain__ub_data_textured_quad_, vs),
			.size = sizeof(struct rain__ub_data_textured_quad_vs_)
		}
	);

	sg_apply_uniforms(
		SG_SHADERSTAGE_FS,
		0,
		&(sg_range){
			.ptr = (uint8_t*)&info + offsetof(struct rain__ub_data_textured_quad_, fs),
			.size = sizeof(struct rain__ub_data_textured_quad_fs_)
		}
	);

	sg_draw(0, 4, 1);
}

void rain_renderer_render_colored_quad(
	struct rain_renderer *restrict this,
	rain_float4 color,
	const rain_float4x4 *restrict transform
) {
	struct rain__ub_data_colored_quad_ info = {
		.vs.quad.trans = *transform,
		.fs.color = color
	};
	// rain__renderer_compute_trans_matrix_(this, &info.vs.quad.trans, transform);

	rain___renderer_bind_pipeline(this, this->builtin_.colored_quad_pipeline);
	rain___renderer_bind_vertex_buffer(this, this->builtin_.quad_vertex_buffer);

	sg_apply_bindings(&this->current_.bind);

	sg_apply_uniforms(
		SG_SHADERSTAGE_VS,
		0,
		&(sg_range){
			.ptr = (uint8_t*)&info + offsetof(struct rain__ub_data_colored_quad_, vs),
			.size = sizeof(struct rain__ub_data_colored_quad_vs_)
		}
	);

	sg_apply_uniforms(
		SG_SHADERSTAGE_FS,
		0,
		&(sg_range){
			.ptr = (uint8_t*)&info + offsetof(struct rain__ub_data_colored_quad_, fs),
			.size = sizeof(struct rain__ub_data_colored_quad_fs_)
		}
	);

	sg_draw(0, 4, 1);
}

