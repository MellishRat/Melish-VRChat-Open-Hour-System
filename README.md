# Melish's VRChat Open Hour System

A flexible UdonSharp scheduling system for VRChat worlds that automatically enables or disables objects, teleports, areas, lights, materials, sounds, and status displays based on configurable days of the week and opening hours.

Designed for:

* Unity 2022.3.22f1 or newer
* VRChat SDK 3.10.4+
* UdonSharp

---

## Features

✅ Enable or disable GameObjects automatically

✅ Separate Open and Closed object lists

✅ Open / Closed status text display

✅ Custom Open and Closed text colours

✅ Countdown timer until opening or closing

✅ Optional display of the next opening day and time

✅ Opening and closing sound effects

✅ Status light colour changes

✅ Optional light enable/disable when open

✅ Mesh material swapping for visual indicators

✅ Configurable time zone offsets

✅ Day-of-week scheduling

✅ Overnight schedules supported (example: 22:00 → 02:00)

✅ Debug display for testing and troubleshooting

---

## Example Uses

### Teleport Access Control

Enable a teleport only during specific opening hours.

Examples:

* Beach access
* VIP rooms
* Event areas
* Seasonal attractions

When closed:

* Teleport is disabled
* Indicator shows CLOSED
* Light turns red

When open:

* Teleport becomes available
* Indicator shows OPEN
* Light turns green

---

### Seasonal Areas

Enable entire sections of a world based on schedules.

Examples:

* Christmas village
* Halloween zone
* Summer beach
* Winter resort

---

### Mini-Games and Activities

Automatically schedule activities.

Examples:

* Pool party
* Bouncy castle
* Whack-a-mole arena
* Dance floor events

---

### Clubs and Venues

Perfect for nightclub-style worlds.

Examples:

* Main dance floor
* DJ booth
* Bar areas
* VIP lounges

---

### Public / Private Areas

Control access to:

* Staff rooms
* Maintenance areas
* Event stages
* Private islands
* Special buildings

---

## Included Example Prefab

The package includes a demonstration prefab showing a complete setup.

The example demonstrates:

* Open / Closed status display
* Countdown timer
* Debug information
* Light colour switching
* Material swapping
* Scheduled object activation

The included example uses two door handles:

* Open Handle
* Closed Handle

These are purely examples.

In a real world you would typically replace them with:

* Teleport triggers
* Udon interaction buttons
* Entire rooms
* Buildings
* Islands
* Attractions
* Mini-games
* Any GameObject that should appear, disappear, or become accessible on a schedule

---

## Time Zone Support

Schedules are not restricted to GMT.

A configurable UTC offset allows creators to schedule using their preferred local time zone.

Examples:

| Location    | UTC Offset |
| ----------- | ---------- |
| UK (Winter) | UTC+00:00  |
| Germany     | UTC+01:00  |
| Japan       | UTC+09:00  |
| US Eastern  | UTC-05:00  |
| US Central  | UTC-06:00  |
| US Pacific  | UTC-08:00  |

---

## Countdown Display

The optional countdown display can show:

### While Closed

```text
CLOSED

Next Opening:
Thursday 00:00 UTC+00:00

Opens In:
48h 12m
```

### While Open

```text
OPEN

Closes In:
3h 45m
```

---

## Setup

### 1. Import the Package

Import the prefab and scripts into your Unity project.

### 2. Add the Controller

Add the **MellishOpenHoursController** component to any GameObject.

### 3. Configure Objects

Add GameObjects to:

* Objects Visible When Open
* Objects Visible When Closed

### 4. Configure Schedule

Set:

* Opening Hour
* Opening Minute
* Closing Hour
* Closing Minute

Choose which days are enabled.

### 5. Optional Features

Assign:

* Status Text
* Countdown Text
* Audio Source
* Open / Close Sounds
* Status Light
* Mesh Renderer
* Open / Closed Materials
* Debug Text

---

## Performance

The controller does not update every frame.

By default it refreshes once every 60 seconds, making it suitable for large worlds with multiple scheduled areas.

---

## License

MIT License

Free to use, modify, redistribute, and include in both public and private VRChat worlds.

---

## Credits

Created by Mellish

Website:
https://www.mellishpenthouse.com

GitHub:
https://github.com/MellishRat

VRChat Community:
https://www.mellishpenthouse.com
