#include <rain/camera.h>
#include <rain/window.h>
#include <rain/renderer.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include "engine.h"
#include "imgui_binds.h"

static struct {
	MonoDomain *domain;
} interop_;

#define RMIF_(NAME) rain_mi_Rain_##NAME

static void RMIF_(Debug_Log)(MonoString *message) {
	char *utf8 = mono_string_to_utf8(message);
	fprintf(stderr, "sr/LOG %s\n", utf8);
	mono_free(utf8);
}

#define RAIN__MI_SETTERF_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, FUNC, ...) \
	static void OBJ##_Set_##FIELD(OBJ_TYPE *o, FIELD_TYPE ref_v) { \
		FUNC(o, __VA_ARGS__ ref_v); }

#define RAIN__MI_SETTER_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, ...) \
	static void rain_mi_Rain_##OBJ##_Set_##FIELD(OBJ_TYPE *o, FIELD_TYPE ref_v) { \
		o->FIELD = __VA_ARGS__ ref_v; }

// for simple "func" getters, which return the value directly.
#define RAIN__MI_GETTERF_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, ...) \
	static FIELD_TYPE rain_mi_Rain_##OBJ##_Get_##FIELD(OBJ_TYPE *o, FIELD_TYPE *out_v) { \
		return o->FIELD##_; }

// for complex "func" getters, which set a pointer to the value.
#define RAIN__MI_GETTERPF_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, ...) \
	static void rain_mi_Rain_##OBJ##_Get_##FIELD(OBJ_TYPE *o, FIELD_TYPE *out_v) { \
		*out_v = o->FIELD##_; }

// for complex getters, which set a pointer to the value.
#define RAIN__MI_GETTERP_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, ...) \
	static void rain_mi_Rain_##OBJ##_Get_##FIELD(OBJ_TYPE *o, FIELD_TYPE *out_v) { \
		*out_v = o->FIELD; }

// for simple getters, which return the value directly.
#define RAIN__MI_GETTER_(OBJ, OBJ_TYPE, FIELD_TYPE, FIELD, ...) \
	FIELD_TYPE rain_mi_Rain_##OBJ##_Get_##FIELD(OBJ_TYPE *o) { \
		return o->FIELD; }


// RAIN__MI_SETTERF_(Camera, struct rain_camera, HMM_Vec3*, position, rain_camera_set_position, *);
// RAIN__MI_GETTERPF_(Camera, struct rain_camera, HMM_Vec3 , position);

// RAIN__MI_SETTERF_(Camera, struct rain_camera, HMM_Quat*, rotation, rain_camera_set_rotation, *);
// RAIN__MI_GETTERPF_(Camera, struct rain_camera, HMM_Quat , rotation);

// RAIN__MI_SETTERF_(Camera, struct rain_camera, float, vert_size, rain_camera_set_vert_size);
// RAIN__MI_GETTERF_(Camera, struct rain_camera, float, vert_size);

// RAIN__MI_SETTERF_(Camera, struct rain_camera, float, fov, rain_camera_set_fov);
// RAIN__MI_GETTERF_(Camera, struct rain_camera, float, fov);

// RAIN__MI_SETTERF_(Camera, struct rain_camera, float, aspect, rain_camera_set_aspect);
// RAIN__MI_GETTERF_(Camera, struct rain_camera, float, aspect);

// RAIN__MI_SETTER_(Renderer, struct rain_renderer, struct rain_camera *, camera);
// RAIN__MI_GETTER_(Renderer, struct rain_renderer, struct rain_camera *, camera);

void RMIF_(Renderer_RenderColoredQuad)(
	struct rain_renderer *o,
	rain_float4 *ref_color,
	rain_float4x4 *ref_trans
) {
	rain_renderer_render_colored_quad(o, *ref_color, ref_trans);
}

struct rain_renderer *RMIF_(Engine_GetRenderer)(unsigned int id) {
	return &rain__engine_.renderer;
}

struct rain_window *RMIF_(Engine_GetWindow)() {
	return &rain__engine_.window;
}

void RMIF_(Window_SetTitle)(struct rain_window *o, MonoString *v) {
	char *utf8 = mono_string_to_utf8(v);
	rain_window_set_title(o, utf8);
	mono_free(utf8);
}

MonoString *RMIF_(Window_GetTitle)(struct rain_window *o) {
	return mono_string_new(interop_.domain, rain_window_get_title(o));
}

bool RMIF_(Window_IsKeyDown)(struct rain_window *o, int keycode) {
	return rain_window_is_key_down(o, keycode);
}

void RMIF_(Window_GetFramebufferSize)(struct rain_window *o, rain_float2 *out_size) {
	int width, height;
	rain_window_get_fb_size(o, &width, &height);
	out_size->x = width;
	out_size->y = height;
}


static struct rain_texture *RMIF_(Texture_Alloc)() {
	return calloc(1, sizeof(struct rain_texture));
}

static void RMIF_(Texture_DestroyAndFree)(struct rain_texture *o) {
	rain_texture_destroy(o);
	free(o);
}

struct RMIF_(Extent2) { unsigned long Width, Height; };

struct RMIF_(TextureDesc) {
	mono_bool IsRenderTarget;
	struct RMIF_(Extent2) Dimensions;
	unsigned int SampleCount;
	unsigned int PixelFormat;
};

static void RMIF_(Texture_Init)(
	struct rain_texture *o,
	struct RMIF_(TextureDesc) *desc
) {
	o->image = sg_make_image(&(sg_image_desc){
		.render_target = desc->IsRenderTarget,
		.pixel_format = desc->PixelFormat,
		.width = desc->Dimensions.Width,
		.height = desc->Dimensions.Height,
		.sample_count = desc->SampleCount,
	});
	o->exists = true;
	o->width = desc->Dimensions.Width;
	o->height = desc->Dimensions.Height;
}

static void RMIF_(Texture_FromFile)(
	struct rain_texture *o,
	MonoString *path,
	int format,
	int usage
) {
	char *utf8_path = mono_string_to_utf8(path);
	rain_texture_from_file(o, utf8_path, format, usage);
	mono_free(utf8_path);
}

static void RMIF_(Texture_GetSize)(
	struct rain_texture *o,
	struct RMIF_(Extent2) *e
) {
	e->Width = o->width;
	e->Height = o->height;
}

static int RMIF_(Texture_GetFormat)(struct rain_texture *o) {
	return o->format;
}

void RMIF_(Window_SetFramebufferSize)(struct rain_window *o, rain_float2 *ref_size) {
	fprintf(stderr, "rr/ERR setting window framebuffer size not supported.\n");
}

static void RMIF_(Renderer_RenderTexturedQuad)(
	struct rain_renderer *o,
	struct rain_texture *tex,
	sg_sampler samp,
	uint64_t offsetX,
	uint64_t offsetY,
	uint64_t width,
	uint64_t height,
	rain_float4x4 *trans
) {
	rain_renderer_render_textured_quad(o, tex, samp, offsetX, offsetY, width, height, trans);
}

static sg_sampler RMIF_(Renderer_GetBuiltinSampler)(
	struct rain_renderer *o,
	[[maybe_unused]] unsigned int id
) {
	return o->builtin_.nearest_sampler;
}

static struct rain_renderer *RMIF_(Renderer_Alloc)() {
	return calloc(1, sizeof(struct rain_renderer));
}

static void RMIF_(Renderer_DestroyAndFree)(struct rain_renderer *o) {
	rain_renderer_deinit(o);
	free(o);
}

struct RMIF_(RenderPass) {
	sg_pass pass;
};

static void RMIF_(Renderer_BeginPass)(
	struct rain_renderer *o,
	struct RMIF_(RenderPass) *pass,
	mono_bool clear,
	rain_float4 *color
) {
	sg_pass_action action = {};
	if (clear) {
		action.colors[0].load_action = SG_LOADACTION_CLEAR;
		action.colors[0].clear_value.r = color->x;
		action.colors[0].clear_value.g = color->y;
		action.colors[0].clear_value.b = color->z;
		action.colors[0].clear_value.a = color->w;
		action.depth.load_action = SG_LOADACTION_CLEAR;
		action.depth.clear_value = 1.0f;
	}
	sg_begin_pass(pass->pass, &action);
}

static void RMIF_(Renderer_BeginDefaultPass)(
	struct rain_renderer *o,
	mono_bool clear,
	rain_float4 *color
) {
	sg_pass_action action = {};
	if (clear) {
		action.colors[0].load_action = SG_LOADACTION_CLEAR;
		action.colors[0].clear_value.r = color->x;
		action.colors[0].clear_value.g = color->y;
		action.colors[0].clear_value.b = color->z;
		action.colors[0].clear_value.a = color->w;
		action.depth.load_action = SG_LOADACTION_CLEAR;
		action.depth.clear_value = 1.0f;
	}
	int width, height;
	rain_window_get_fb_size(o->window, &width, &height);
	sg_begin_default_pass(&action, width, height);
}

static void RMIF_(Renderer_EndPass)(
	struct rain_renderer *o
) {
	sg_end_pass();
}

static void RMIF_(ImGUI_Init)(
	unsigned long maxVertices
) {
	rain_imgui_init(maxVertices);
}

static void RMIF_(ImGUI_DeInit)() {
	rain_imgui_deinit();
}

static void RMIF_(ImGUI_BeginRender)() {
	rain_imgui_begin_render();
}

static void RMIF_(ImGUI_EndRender)() {
	rain_imgui_end_render();
}

static void RMIF_(ImGUI_Begin)(
	MonoString *name,
	mono_bool *isOpen
) {
	char *utf8_name = mono_string_to_utf8(name);
	// bool should be smaller than int!
	rain_imgui_begin(utf8_name, (bool*)isOpen);
	mono_free(utf8_name);
}

static void RMIF_(ImGUI_End)() {
	rain_imgui_end();
}

void rain_bind_mono_interop(MonoDomain *domain) {
	interop_.domain = domain;

#define RAIN__ADD_ICALL_(NAME) \
	mono_add_internal_call("RainNative.Interop::" #NAME, &RMIF_(NAME))
	RAIN__ADD_ICALL_(Debug_Log);

	RAIN__ADD_ICALL_(Renderer_RenderColoredQuad);
	RAIN__ADD_ICALL_(Renderer_RenderTexturedQuad);
	RAIN__ADD_ICALL_(Renderer_GetBuiltinSampler);
	RAIN__ADD_ICALL_(Renderer_Alloc);
	RAIN__ADD_ICALL_(Renderer_DestroyAndFree);
	RAIN__ADD_ICALL_(Renderer_BeginPass);
	RAIN__ADD_ICALL_(Renderer_BeginDefaultPass);
	RAIN__ADD_ICALL_(Renderer_EndPass);

	RAIN__ADD_ICALL_(Engine_GetWindow);
	RAIN__ADD_ICALL_(Window_SetTitle);
	RAIN__ADD_ICALL_(Window_GetTitle);
	RAIN__ADD_ICALL_(Window_SetFramebufferSize);
	RAIN__ADD_ICALL_(Window_GetFramebufferSize);
	RAIN__ADD_ICALL_(Window_IsKeyDown);

	RAIN__ADD_ICALL_(Texture_Alloc);
	RAIN__ADD_ICALL_(Texture_DestroyAndFree);
	RAIN__ADD_ICALL_(Texture_FromFile);
	RAIN__ADD_ICALL_(Texture_Init);
	RAIN__ADD_ICALL_(Texture_GetSize);
	RAIN__ADD_ICALL_(Texture_GetFormat);

	RAIN__ADD_ICALL_(ImGUI_DeInit);
	RAIN__ADD_ICALL_(ImGUI_BeginRender);
	RAIN__ADD_ICALL_(ImGUI_EndRender);
	RAIN__ADD_ICALL_(ImGUI_Begin);
	RAIN__ADD_ICALL_(ImGUI_End);
}
