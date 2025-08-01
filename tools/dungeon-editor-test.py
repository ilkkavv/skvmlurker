import pygame
import sys
import csv

# Grid settings
GRID_WIDTH = 32
GRID_HEIGHT = 32
TILE_SIZE = 16

# Colors
BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
BLUE = (0, 0, 255)
GRID_LINE = (50, 50, 50)

# Tile codes
TILE_BLACK = '#'
TILE_WHITE = '.'
TILE_BLUE = '~'

# Initialize pygame
pygame.init()
window_size = (GRID_WIDTH * TILE_SIZE, GRID_HEIGHT * TILE_SIZE)
screen = pygame.display.set_mode(window_size)
pygame.display.set_caption("Dungeon Grid Editor")

# Clock for smooth dragging
clock = pygame.time.Clock()

# Create grid: store characters
grid = [[TILE_BLACK for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]

# Track mouse drag
mouse_down = False
mouse_button = 0

def set_tile(x, y, button):
    if 0 <= x < GRID_WIDTH and 0 <= y < GRID_HEIGHT:
        if button == 1:  # Left click → white
            grid[y][x] = TILE_WHITE
        elif button == 3:  # Right click → black
            grid[y][x] = TILE_BLACK
        elif button == 2:  # Middle click → blue
            grid[y][x] = TILE_BLUE

def export_csv():
    with open("dungeon-level.csv", "w", newline='') as file:
        writer = csv.writer(file)
        for row in grid:
            writer.writerow(row)
    print("Exported dungeon-level.csv")

def import_csv():
    try:
        with open("dungeon-level.csv", newline='') as file:
            reader = csv.reader(file)
            for y, row in enumerate(reader):
                if y < GRID_HEIGHT:
                    for x, val in enumerate(row):
                        if x < GRID_WIDTH:
                            grid[y][x] = val
        print("Imported dungeon-level.csv")
    except FileNotFoundError:
        print("File dungeon-level.csv not found.")

# Main loop
running = True
while running:
    clock.tick(60)  # Limit to 60 FPS
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

        # Mouse button down
        elif event.type == pygame.MOUSEBUTTONDOWN:
            mouse_down = True
            mouse_button = event.button
            mx, my = pygame.mouse.get_pos()
            set_tile(mx // TILE_SIZE, my // TILE_SIZE, mouse_button)

        # Mouse button up
        elif event.type == pygame.MOUSEBUTTONUP:
            mouse_down = False

        # Keyboard press
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_e:
                export_csv()
            elif event.key == pygame.K_i:
                import_csv()

    # Dragging while mouse held
    if mouse_down:
        mx, my = pygame.mouse.get_pos()
        set_tile(mx // TILE_SIZE, my // TILE_SIZE, mouse_button)

    # Draw grid
    screen.fill(GRID_LINE)
    for y in range(GRID_HEIGHT):
        for x in range(GRID_WIDTH):
            color = {
                TILE_BLACK: BLACK,
                TILE_WHITE: WHITE,
                TILE_BLUE: BLUE
            }.get(grid[y][x], BLACK)

            rect = pygame.Rect(x * TILE_SIZE + 1, y * TILE_SIZE + 1, TILE_SIZE - 1, TILE_SIZE - 1)
            pygame.draw.rect(screen, color, rect)

    pygame.display.flip()

# Clean up
pygame.quit()
sys.exit()
