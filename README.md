# Lovely World

## TODO

- Bugs
	- spawn thump is back
	- enemies get stuck walking against items- turn around? or push? or ignore? (cant because then they couldnt carry items)
	- enemies standing on top of items is weird
	- enemy drop off corner- is this the wrong drop check issue?
	- don't animate enemy if turnaround on both sides
	- give a little up vel when exiting teleport jar? (need to move player rb assignment from start to awake)
	- fix check for free area when throwing/dropping
		- push out of collision if possible?
	- thrown solids will slide along nonsolid items- should all items just be solid?
	- thrown item landing on head is weird
		- gets stuck in midair
- moving platforms (run/jump/throw momentum)
	- if standing on solid rigidbody with velocity, take its velocity
	- includes enemies (& other holdables)
	- applies to players & holdables
- enemies
	- player damage when hitting side/bottom
		- hurt trigger box (bigger than hold box)
		- no damage when holding! what about falling?
	- place down: face away from you so you don't take damage?
	- bugs- worm, grasshopper
	- some can't be picked up (spiky back)
- items
	- magic wand
		- animate/deanimate fruits and veggies
			- collide with Animatable: switch it with its stored prefab
		- other weird uses
		- E or Z to use? or up? or just throw?
	- keys & locked doors
	- backpack
		- stand on & press down while holding- put item in backpack
		- can hold multiple items at once
		- extract by stand on & press down? or up? or use while holding?
		- enter urn while its partner is in the backpack- paradox if backpack held, enter backpack zone if not
			- theres a goblin npc inside who sorts all the items
			- "sketchy shopkeep" music
	- ladders
	- torches- melt ice above you, light in darkness
		- throw and slide along ice puzzle
	- parachute- slow fall to cross gaps
		- throw across gap puzzle
		- glide back to wall for multiple walljumps
	- frog with sticky tongue- grapple on ceiling
		- activated by jump key? or up/down? or on timer?
	- bridge that lets you go through blocks
- background art
- collectible hearts?
	- juicier texture
	- animate up/down
	- collect animation
- health
	- collectible heart fragments
	- checkpoints
	- if you're inside door or movable solid item after reset/recall, it squishes you and you respawn
		- squishzone inside player
- bosses
- one way platforms
- up key: look up?
- scene transition fx
- screen scroll
- palettes are unlockable items
	- silhouette palette like playerfall
	- gameboy green palette
- saving/loading
- particles
	- make sure they're pixel aligned
	- teleport
	- recall/reset crystal smash
- screen shake
- disable walljump? maybe an item enables this?
- UI
	- pause
- animations
- story
- title screen
	- logo
- credits roll listing all the enemies
- slippery ice
- item spawn rule manipulation
- npcs with dialog (mintty)
	- can pick them up and they go "hey put me down!"
	- trade them items
- doors? to dream world
- throw object in midair pushes you back?- sorta double jump

- design
	- everything serves at least two purposes
	- progression: world state change, carried item, knowledge of secrets, collectible hearts
	- non euclidean room transitions?

- world
	- center- veggie garden + love + magic (starting area) (slam-funk) (red hearts)
	- left- tide pools & cliffs (octopus, crab etc.) + sunset
	- up- popsicle ice zone (fruits?)
	- right- gears jungle (robots, moving/rotating platforms) (juhani stage 2)
	- way up- saturn palace
	- down?- hollow earth cave (bigfoot, faceless birds, atlantis etc.)
	- limbo
		- big & scrollable, enter with long fall
		- some stuff changes as you progress
		- secret puzzle to carry out void tear
		- set limbo palette override
		- enter/leave animation & sound
		- limbo music start right away
	- dream world
		- connection to limbo
	- secret lagoon
	- slumbering beast
- other heart colors: orange, yellow, green, blue, purple, white

- puzzles
	- enemies
		- put item on back of enemy to move it
		- ride on flying enemy for a long time
		- ride enemy across spikes into secret room
		- carry enemy and drop in another room to use its behavior
	- get around room reset mechanic
	- crystals
		- throw reset/recall crystal and quickly grab another item
		- put down and regrab recall crystal to change its recall point
		- use key from one room on multiple doors in another
	- teleport jar
		- take teleport vase into its partner and enter limbo world
		- teleport across scenes
		- rotate jar to come out gravity rotated?
	- backpack
		- put multiple items in backpack
		- take teleport vase into its partner in backpack

- pre-launch
	- make sure all music is preload
	- unmute persistent music

## Credits

- Music
	- "Slam-Funk" composed and arranged by Haley Halcyon: https://opengameart.org/content/nes-chiptune-slam-funk
	- "4 Chiptunes (Adventure)" by SubspaceAudio: https://opengameart.org/content/4-chiptunes-adventure
	- "Angel Eyes (Chiptune Edit)" by Kim Lightyear: https://opengameart.org/content/angel-eyes-chiptune-edit
	- "Lost in a bad place (horror ambience loop)" by congusbongus: https://opengameart.org/content/lost-in-a-bad-place-horror-ambience-loop
	- "The Sketchy Shopkeep" by Cal McEachern: https://opengameart.org/content/chiptune-rpg-soundtrack-6-loops
- Sound effects
	- "Sound effects Mini Pack1.5" by phoenix1291: https://opengameart.org/content/sound-effects-mini-pack15
	- "512 Sound Effects (8-bit style)" by SubspaceAudio: https://opengameart.org/content/512-sound-effects-8-bit-style
	- Others generated with sfxr: https://sfxr.me/
- Palettes
	- "Island Joy 16x" by Kerrie Lake: https://lospec.com/palette-list/island-joy-16
	- Others from https://lospec.com/palette-list
