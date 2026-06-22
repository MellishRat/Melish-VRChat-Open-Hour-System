# Melish's VRChat Open Hour System

A lightweight **UdonSharp open-hours controller** for VRChat worlds.

Originally made for **Melish's Goon Cave Club**, where parts of the world can open, close, change status displays, play sounds, alter lights, and show a countdown based on scheduled opening hours.

## Target setup

- Unity `2022.3.22f1`
- VRChat World SDK `3.10.4`
- UdonSharp

This is an unofficial community tool and is not affiliated with VRChat Inc.

## What it does

`MellishOpenHoursController.cs` can:

- Show objects while open
- Show different objects while closed
- Update a UI `Text` status label
- Use separate open/closed text colours
- Show an optional countdown until the next opening time
- Play an opening sound once when opening
- Play a closing sound once when closing
- Change an optional status light colour
- Optionally enable the light only while open
- Check selected days of the week
- Use a configurable reference time zone instead of being locked to GMT
- Handle overnight opening hours, such as `22:00` to `02:00`
- Treat matching open/close times as an all-day opening for enabled days
- Display optional debug information

## Example uses

- Nightclub open/closed sign
- Beach mode that only appears on certain days
- Pool night setup
- VIP room schedule
- Event room open hours
- Seasonal or themed prop toggles
- Status lights for doors, signs, or entrance areas
- Countdown display for upcoming events

## Installation

1. Create or open a Unity project using Unity `2022.3.22f1`.
2. Install the VRChat World SDK and UdonSharp.
3. Copy the folder `Assets/MelishOpenHoursSystem` into your Unity project's `Assets` folder.
4. Add `MellishOpenHoursController` to a GameObject in your scene.
5. Assign your open objects, closed objects, status text, countdown text, audio source, sounds, and optional light.
6. Set your reference time zone, opening time, closing time, and open days.

## Reference time zone

The script uses `Networking.GetNetworkDateTime()` as its stable base time, then applies your chosen reference time zone offset.

Examples:

- UTC / GMT: `0 hours`, `0 minutes`
- UK summer / BST: `1 hour`, `0 minutes`
- Japan / JST: `9 hours`, `0 minutes`
- US Eastern winter / EST: `-5 hours`, `0 minutes`
- US Eastern summer / EDT: `-4 hours`, `0 minutes`
- US Pacific winter / PST: `-8 hours`, `0 minutes`
- US Pacific summer / PDT: `-7 hours`, `0 minutes`
- India / IST: `5 hours`, `30 minutes`

VRChat/Udon does not automatically know your intended real-world daylight saving rules, so set the offset you want your world schedule to follow.

## Countdown display

Assign a UI `Text` object to **Countdown Text** to show the time until the next enabled opening period.

Default example:

```text
Opens in 48h 00m
```

You can choose between:

- Total hours and minutes, such as `51h 20m`
- Days, hours and minutes, such as `2d 03h 20m`

When the area is already open, the countdown can show `Open now` or be hidden.

## Overnight hours

Overnight ranges are supported.

Example:

- Opening: `22:00`
- Closing: `02:00`
- Friday enabled

This means the system opens on **Friday at 22:00 reference time** and stays open until **Saturday at 02:00 reference time**.

You do not need to enable Saturday unless you also want a separate Saturday opening period.

## Inspector guide

### Objects To Toggle

- **Objects Visible When Open**: enabled while the schedule is open.
- **Objects Visible When Closed**: enabled while the schedule is closed.

### Status Display

- Optional UI Text label.
- Custom open/closed messages.
- Custom open/closed colours.

### Countdown Display

- Optional UI Text label for the next opening countdown.
- Custom prefix, open message, and no-upcoming-opening message.
- Toggle between total-hours format and days/hours/minutes format.

### Audio Cues

- Optional AudioSource.
- Opening and closing sounds play only when the state changes.
- First-load sound can be enabled or disabled.

### Status Light

- Optional Unity Light.
- Can change colour based on state.
- Can either stay enabled or only turn on while open.

### Reference Time Zone

Set the schedule's friendly time zone label and UTC offset. This lets the world author choose whether the schedule follows UK time, US time, Japan time, server time, or any other fixed offset.

### Update Settings

The default check interval is `60` seconds. That is usually enough for world open/closed logic and avoids unnecessary constant checks.

## License

MIT License. See `LICENSE`.
