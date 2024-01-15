import random
import re

class Minesweeper:
    def __init__(self, width, height, num_mines):
        self.width = width
        self.height = height
        self.board = [[' ' for _ in range(width)] for _ in range(height)]
        self.mines = set()
        self.uncovered = set()
        self.num_mines = num_mines

    def place_mines(self):
        while len(self.mines) < self.num_mines:
            x, y = random.randint(0, self.width - 1), random.randint(0, self.height - 1)
            if (x, y) not in self.mines:
                self.mines.add((x, y))

    def is_mine(self, x, y):
        return (x, y) in self.mines

    def adjacent_mines(self, x, y):
        count = 0
        for dx in [-1, 0, 1]:
            for dy in [-1, 0, 1]:
                if (dx, dy) != (0, 0) and 0 <= x + dx < self.width and 0 <= y + dy < self.height:
                    count += (x + dx, y + dy) in self.mines
        return count

    def uncover(self, x, y):
        if self.is_mine(x, y):
            return False
        self.uncovered.add((x, y))
        if self.adjacent_mines(x, y) == 0:
            for dx in [-1, 0, 1]:
                for dy in [-1, 0, 1]:
                    if 0 <= x + dx < self.width and 0 <= y + dy < self.height and (x + dx, y + dy) not in self.uncovered:
                        self.uncover(x + dx, y + dy)
        return True

    def print_board(self, show_mines=False):
        for y in range(self.height):
            for x in range(self.width):
                if (x, y) in self.uncovered:
                    print(self.adjacent_mines(x, y) or '.', end=' ')
                elif show_mines and self.is_mine(x, y):
                    print('*', end=' ')
                else:
                    print('?', end=' ')
            print()
        print()

    def play(self):
        self.place_mines()
        while True:
            self.print_board()
            try:
                x, y = map(int, re.findall(r'\d+', input('Enter coordinates (x y): ')))
                assert 0 <= x < self.width and 0 <= y < self.height
            except (ValueError, AssertionError):
                print('Invalid input. Please enter x and y coordinates like "2 3".')
                continue

            if not self.uncover(x, y):
                self.print_board(show_mines=True)
                print('Game Over! You hit a mine.')
                break
            elif len(self.uncovered) == self.width * self.height - self.num_mines:
                print('Congratulations! You have cleared all the mines.')
                break

if __name__ == "__main__":
    game = Minesweeper(10, 10, 10)
    game.play()