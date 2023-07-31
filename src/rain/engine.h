#ifndef RAIN__ENGINE_H_
#define RAIN__ENGINE_H_

#include <rain/window.h>
#include <rain/renderer.h>

extern struct rain_engine {
	struct rain_window window;
	struct rain_renderer renderer;
	float delta_time;
} rain__engine_;

#endif // RAIN__ENGINE_H_
