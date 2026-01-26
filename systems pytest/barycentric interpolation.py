import pygame
import math
import sys

# Инициализация Pygame
pygame.init()

# Размеры окна
WIDTH, HEIGHT = 800, 700
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Треугольник с барицентрическими координатами")

# Цвета
BACKGROUND = (20, 20, 30)
VERTEX_A_COLOR = (255, 50, 50)    # Красный для A
VERTEX_B_COLOR = (50, 255, 50)    # Зеленый для B
VERTEX_C_COLOR = (50, 100, 255)   # Синий для C
POINT_COLOR = (255, 255, 255)
TEXT_COLOR = (240, 240, 240)
GRID_COLOR = (40, 40, 60)

# Масштаб для отображения треугольника
SCALE = 180  # Уменьшим масштаб, чтобы весь треугольник помещался
CENTER_X, CENTER_Y = WIDTH // 2, HEIGHT // 2 + 100

# Вершины треугольника (математические координаты)
A = (0, math.sqrt(3))
B = (-1, 0)
C = (1, 0)

# Преобразование математических координат в экранные
def to_screen(x, y):
    return (CENTER_X + x * SCALE, CENTER_Y - y * SCALE)

# Преобразование экранных координат в математические
def to_math(screen_x, screen_y):
    x = (screen_x - CENTER_X) / SCALE
    y = (CENTER_Y - screen_y) / SCALE
    return x, y

# Проверка, находится ли точка внутри треугольника
def inside_triangle(x, y):
    if abs(x) > 1:
        return False
    return 0 <= y <= math.sqrt(3) * (1 - abs(x))

# Барицентрические координаты
def barycentric_coords(x, y):
    u = y / math.sqrt(3)            # близость к вершине A (0, sqrt(3))
    v = (1 - u - x) / 2.0           # близость к вершине B (-1, 0)
    w = (1 - u + x) / 2.0           # близость к вершине C (1, 0)
    return (u, v, w)

# Получение цвета на основе барицентрических координат
def get_color_from_barycentric(u, v, w):
    r = int(u * VERTEX_A_COLOR[0] + v * VERTEX_B_COLOR[0] + w * VERTEX_C_COLOR[0])
    g = int(u * VERTEX_A_COLOR[1] + v * VERTEX_B_COLOR[1] + w * VERTEX_C_COLOR[1])
    b = int(u * VERTEX_A_COLOR[2] + v * VERTEX_B_COLOR[2] + w * VERTEX_C_COLOR[2])
    return (r, g, b)

# Основные переменные
current_point = (0, 0.5)  # Начальная точка
dragging = False

# Шрифт
font = pygame.font.SysFont('Arial', 20)
title_font = pygame.font.SysFont('Arial', 28, bold=True)

def draw_grid():
    # Рисуем сетку с учётом размеров треугольника
    # Вершина A имеет y = √3 ≈ 1.732, поэтому нужен диапазон по Y от -0.5 до 2.0
    for i in range(-3, 4):
        # Вертикальные линии
        x_val = i * 0.5
        start_pos = to_screen(x_val, -0.5)
        end_pos = to_screen(x_val, 2.0)
        pygame.draw.line(screen, GRID_COLOR, start_pos, end_pos, 1)
        
        # Горизонтальные линии
        y_val = i * 0.5
        start_pos = to_screen(-1.5, y_val)
        end_pos = to_screen(1.5, y_val)
        pygame.draw.line(screen, GRID_COLOR, start_pos, end_pos, 1)
    
    # Ось X
    pygame.draw.line(screen, (100, 100, 150), 
                     to_screen(-1.5, 0), to_screen(1.5, 0), 2)
    # Ось Y
    pygame.draw.line(screen, (100, 100, 150), 
                     to_screen(0, -0.5), to_screen(0, 2.0), 2)

def draw_triangle_with_colors():
    # ИСПРАВЛЕНО: Рисуем треугольник правильно, используя алгоритм сканлайн
    # или просто рисуем много пикселей, охватывая весь треугольник
    
    # Определяем границы треугольника в математических координатах
    min_x, max_x = -1, 1
    min_y, max_y = 0, math.sqrt(3)
    
    # Определяем границы в экранных координатах
    screen_min_x, screen_max_y = to_screen(min_x, max_y)  # Левый верхний угол
    screen_max_x, screen_min_y = to_screen(max_x, min_y)  # Правый нижний угол
    
    # Рисуем с шагом 2 пикселя для производительности
    step = 2
    for screen_y in range(int(screen_min_y), int(screen_max_y) + 1, step):
        for screen_x in range(int(screen_min_x), int(screen_max_x) + 1, step):
            # Преобразуем в математические координаты
            x, y = to_math(screen_x, screen_y)
            
            if inside_triangle(x, y):
                u, v, w = barycentric_coords(x, y)
                color = get_color_from_barycentric(u, v, w)
                
                # Рисуем квадрат step×step
                pygame.draw.rect(screen, color, (screen_x, screen_y, step, step))

def draw_triangle_with_colors_fast():
    # Более быстрая версия: рисуем треугольник через полигон и используем
    # заливку с интерполяцией цветов вершин
    a_screen = to_screen(*A)
    b_screen = to_screen(*B)
    c_screen = to_screen(*C)
    
    # Создаём поверхность для плавной заливки
    triangle_surface = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    
    # Рисуем градиентный треугольник методом сканирования строк
    height = int(c_screen[1] - a_screen[1])  # Высота в пикселях
    if height <= 0:
        return
    
    for y in range(int(a_screen[1]), int(c_screen[1]) + 1):
        # Находим x-координаты на левой и правой границах для этой y
        # Левая сторона (A-B)
        t1 = (y - a_screen[1]) / (b_screen[1] - a_screen[1]) if b_screen[1] != a_screen[1] else 0
        x_left = a_screen[0] + (b_screen[0] - a_screen[0]) * t1
        
        # Правая сторона (A-C)
        t2 = (y - a_screen[1]) / (c_screen[1] - a_screen[1]) if c_screen[1] != a_screen[1] else 0
        x_right = a_screen[0] + (c_screen[0] - a_screen[0]) * t2
        
        # Для нижней части треугольника
        if y > b_screen[1]:
            # Левая сторона (B-C)
            t1 = (y - b_screen[1]) / (c_screen[1] - b_screen[1]) if c_screen[1] != b_screen[1] else 0
            x_left = b_screen[0] + (c_screen[0] - b_screen[0]) * t1
        
        # Сортируем, чтобы x_left был левее x_right
        if x_left > x_right:
            x_left, x_right = x_right, x_left
        
        # Рисуем горизонтальную линию
        if x_right > x_left:
            for x in range(int(x_left), int(x_right) + 1, 2):
                # Преобразуем в математические координаты
                math_x, math_y = to_math(x, y)
                if inside_triangle(math_x, math_y):
                    u, v, w = barycentric_coords(math_x, math_y)
                    color = get_color_from_barycentric(u, v, w)
                    pygame.draw.rect(triangle_surface, color, (x, y, 2, 2))
    
    screen.blit(triangle_surface, (0, 0))

def draw_vertices():
    # Рисуем вершины
    a_pos = to_screen(*A)
    b_pos = to_screen(*B)
    c_pos = to_screen(*C)
    
    # Вершина A (красная)
    pygame.draw.circle(screen, VERTEX_A_COLOR, (int(a_pos[0]), int(a_pos[1])), 12)
    # Вершина B (зеленая)
    pygame.draw.circle(screen, VERTEX_B_COLOR, (int(b_pos[0]), int(b_pos[1])), 12)
    # Вершина C (синяя)
    pygame.draw.circle(screen, VERTEX_C_COLOR, (int(c_pos[0]), int(c_pos[1])), 12)
    
    # Подписи вершин
    text_a = font.render("A (0, √3)", True, VERTEX_A_COLOR)
    text_b = font.render("B (-1, 0)", True, VERTEX_B_COLOR)
    text_c = font.render("C (1, 0)", True, VERTEX_C_COLOR)
    
    screen.blit(text_a, (a_pos[0] + 15, a_pos[1] - 10))
    screen.blit(text_b, (b_pos[0] - 70, b_pos[1] + 10))
    screen.blit(text_c, (c_pos[0] + 10, c_pos[1] + 10))

def draw_current_point():
    # Рисуем текущую точку
    screen_x, screen_y = to_screen(*current_point)
    pygame.draw.circle(screen, POINT_COLOR, (int(screen_x), int(screen_y)), 8)
    pygame.draw.circle(screen, (0, 0, 0), (int(screen_x), int(screen_y)), 8, 2)

def draw_info():
    # Отображаем информацию о текущей точке
    x, y = current_point
    inside = inside_triangle(x, y)
    u, v, w = barycentric_coords(x, y) if inside else (0, 0, 0)
    
    # Фон для текста
    pygame.draw.rect(screen, (30, 30, 40, 200), (20, 20, 360, 160))
    pygame.draw.rect(screen, (60, 60, 80), (20, 20, 360, 160), 2)
    
    # Заголовок
    title = title_font.render("Барицентрические координаты", True, TEXT_COLOR)
    screen.blit(title, (30, 30))
    
    # Координаты точки
    coord_text = font.render(f"Точка: ({x:.3f}, {y:.3f})", True, TEXT_COLOR)
    inside_text = font.render(f"Внутри треугольника: {'ДА' if inside else 'НЕТ'}", 
                             True, (50, 255, 100) if inside else (255, 100, 100))
    
    screen.blit(coord_text, (30, 70))
    screen.blit(inside_text, (30, 100))
    
    if inside:
        # Барицентрические координаты
        bary_text = font.render(f"u (к A): {u:.3f}", True, VERTEX_A_COLOR)
        screen.blit(bary_text, (30, 130))
        
        bary_text = font.render(f"v (к B): {v:.3f}", True, VERTEX_B_COLOR)
        screen.blit(bary_text, (30, 155))
        
        bary_text = font.render(f"w (к C): {w:.3f}", True, VERTEX_C_COLOR)
        screen.blit(bary_text, (30, 180))
        
        # Сумма координат
        sum_text = font.render(f"Сумма (u+v+w): {u+v+w:.3f}", True, TEXT_COLOR)
        screen.blit(sum_text, (200, 130))
        
        # Цвет точки
        color = get_color_from_barycentric(u, v, w)
        color_preview = pygame.Rect(200, 160, 40, 40)
        pygame.draw.rect(screen, color, color_preview)
        pygame.draw.rect(screen, TEXT_COLOR, color_preview, 2)
        
        color_text = font.render(f"RGB: {color}", True, TEXT_COLOR)
        screen.blit(color_text, (250, 165))
    
    # Инструкция
    instr_text = font.render("Перетаскивайте точку мышью. ESC для выхода.", True, (150, 150, 180))
    screen.blit(instr_text, (WIDTH // 2 - 180, HEIGHT - 40))

# Основной цикл
clock = pygame.time.Clock()
running = True

while running:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False
        
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_ESCAPE:
                running = False
        
        elif event.type == pygame.MOUSEBUTTONDOWN:
            if event.button == 1:  # Левая кнопка мыши
                mouse_x, mouse_y = event.pos
                math_x, math_y = to_math(mouse_x, mouse_y)
                
                # Проверяем, кликнули ли рядом с текущей точкой
                screen_x, screen_y = to_screen(*current_point)
                distance = math.sqrt((mouse_x - screen_x)**2 + (mouse_y - screen_y)**2)
                
                if distance < 20:  # Если клик рядом с точкой
                    dragging = True
        
        elif event.type == pygame.MOUSEBUTTONUP:
            if event.button == 1:
                dragging = False
        
        elif event.type == pygame.MOUSEMOTION:
            if dragging:
                mouse_x, mouse_y = event.pos
                current_point = to_math(mouse_x, mouse_y)
    
    # Отрисовка
    screen.fill(BACKGROUND)
    
    draw_grid()
    draw_triangle_with_colors_fast()  # Используем улучшенную версию
    draw_vertices()
    draw_current_point()
    draw_info()
    
    pygame.display.flip()
    clock.tick(60)

pygame.quit()
sys.exit()