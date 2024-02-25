import sys
import math
from PIL import Image
from munkres import Munkres

def dist(a, b):
    return math.sqrt((a[0] - b[0]) ** 2 + (a[1] - b[1]) ** 2 + (a[2] - b[2]) ** 2)

def main():
    base = sys.argv[1]
    edit = sys.argv[2]
    base_img = Image.open(base)
    edit_img = Image.open(edit)
    base_colors = []
    edit_colors = []
    for x in range(16):
        base_colors.append(base_img.getpixel((x, 0)))
        edit_colors.append(edit_img.getpixel((x, 0)))
    
    matrix = []
    for i in range(16):
        row = []
        for j in range(16):
            val = dist(base_colors[i], edit_colors[j])
            row.append(val)
        matrix.append(row)
    
    m = Munkres()
    indices = m.compute(matrix)
    final_colors = [0] * 16
    for ind in indices:
        final_colors[ind[0]] = edit_colors[ind[1]]
    
    for x in range(16):
        edit_img.putpixel((x, 0), final_colors[x])
    edit_img.save(edit)

if __name__ == "__main__":
    main()
