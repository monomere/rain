#ifndef RAIN__WINDOW_H_
#define RAIN__WINDOW_H_
#include <rain/compat.h>

struct rain_window {
	struct rain__window_handle_ *handle;
};

/** initialize window. */
void rain_window_init(
	struct rain_window *RAIN_RESTRICT this_,
	const char *RAIN_RESTRICT title,
	int width, int height
);

/** end frame. */
void rain_window_frame(struct rain_window *this_);

/** should the window close (loop condition). */
bool rain_window_should_close(struct rain_window *this_);

/** get sokol context. */
struct sg_context_desc rain_window_get_context(struct rain_window *this_);

/** get the size of the framebuffer.
    not the same as the window size. */
void rain_window_get_fb_size(
	struct rain_window *RAIN_RESTRICT this_,
	int *RAIN_RESTRICT width, int *RAIN_RESTRICT height
);

/** deinitialize window. */
void rain_window_deinit(struct rain_window *this_);

/** get time since window creation (TODO: revise) in seconds. */
float rain_window_get_time(struct rain_window *this_);

/** return true if a key is pressed down. */
bool rain_window_is_key_down(struct rain_window *this_, int keycode);

const char *rain_window_get_title(const struct rain_window *this_);

void rain_window_set_title(
	struct rain_window *RAIN_RESTRICT this_,
	const char *RAIN_RESTRICT title
);

#endif // RAIN__WINDOW_H_
