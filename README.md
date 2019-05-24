# EQHelper
EQ Bot for Leveling and camping drops

This repository isn't in great shape for sharing -- it was a personal project to auto-farm in EQ, first on my desktop, then on my laptop, then both in tandem (one fire shielding on my mage to power-level some other characters), then running both accounts on the same computer.

Necessary improvements before I'd be proud of this:

- Update the slack helper to load the web hook from a file for security (webhook has been regenerated since checkin :P), and only create the single client
- Offload a lot of the config stuff to some sort of ini file (names, coordinates of hp/mana bars, etc.)
- Add some validation to make sure your hotbar doesn't conflict with itself (some keys are re-used between tasks so there's a bunch of pitfalls)
- Fix the multi-account handling to be cleaner (rather than EQTasks returning true or false, they should return some sort of more complicated struct with success/amount of time to wait before that character's thread picks up again, use an actual locking mechanism instead of a currently active char name, etc.)
- Get hp/mana/etc. UI working with multi-account
- Refactor gameplay loops.  There's common patterns -- init/do core functionality/panic mode/etc. which should be broken out so ideally a non-programmer could specify through config
- I'm sure more I'm not thinking about at the moment.