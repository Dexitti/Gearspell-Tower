import math

def inside_triangle(x, y):
    if abs(x) > 1:
        return False
    return 0 <= y <= math.sqrt(3) * (1 - abs(x))

def barycentric_coords(x, y):
    # Предполагаем, что точка (x,y) лежит внутри равностороннего треугольника
    # из условий: 0 <= y <= sqrt(3)*(1 - |x|), |x| <= 1
    u = y / math.sqrt(3)            # близость к вершине A (0, sqrt(3))
    v = (1 - u - x) / 2.0           # близость к вершине B (-1, 0)
    w = (1 - u + x) / 2.0           # близость к вершине C (1, 0)
    return (u, v, w)                # все значения от 0 до 1, сумма = 1

x, y = map (float, input ("Введите координаты точки: ").split ())
print(inside_triangle(x, y))
print(barycentric_coords(x, y))