   M S B A M L   `   `   `  ����H  DIndividualLab, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:IndividualLabIndividualLab  5   	       IndividualLab.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:IndividualLab     5   	   -   5          � WindowStyle$
  None=�5       � WindowStartupLocation$ CenterScreen=�5   	    � Title$ 
MainWindow��5	   	   $	��830��6   $
��1200��6)    � 
ResizeMode$ NoResize=�66   '  !System.Windows.Shell.WindowChrome   WindowChrome 5
        5   
      GlassFrameThickness$ 0q�6      CornerRadius$ 0��6/      CaptionHeight$ 0Q�6@   .��5      � -   5         �Name$ mainWindowGrid��5      	 � RowDefinitions	 5   
   �� 5      $��30 �6   �� 5      .+�5   
   ]� $��0��6   
�������$	��Top=�68   .+�5      o� $
��Left=�6   
�������.@�5      m� -   5        
 m�Name$
 menuItem��5      $��Файл��6-   $��30��6;   $��50��6G   $
��Left=�6R   $��16(�6t   .@�5      m� -   5        $
 openItem��5      $��Открыть��61   6B   m� 5      -   5        $
 saveItem��5      $��Сохранить��61   6D   5      5      �� 5      -   5         ��Name$ fileNameLabel��5      $
��1000��6+   �� 5      $��Right=�6    �� Orientation$ 
Horizontal=�60   $��Right=�6I   .+�5      �� -   5         ��Name$ btnMinimize��5      $��-��6.   $��30��6:   $��30��6E   
������ 
������ 5   .   �� 5      -   5        $ btnMaximize��5      $
��🗖��6.   $��30��6;   $��30��6F   
������ 
������ 5    .   �� 5!      -	   5        $ btnClose��5!      $��X��6+   $��30��67   $��30��6B   
������ 
������ 5"   .   5#      5$      � 5&   
   -
   5        $ mainGrid��5&      $��1��6!    � ColumnDefinitions 5'      �� 5(      $	��200 �6#   �� 5)      $��* �6#   .+�5,      � 	 5-      �� 5.      $	��630 �6$   �� 5/      $	��200 �6$   .+�52      �� -   5         ��Name$ inputStackPanel��52      $��0��66   $��0��6F   .+�53      �� $%��Число переменных��6   $��30��67   $	��200��6C   $��Center=�6O   $��14(�6s   $��Times New Roman)�6�   �� 54      -   5         ��Name$ amountOfXTextBox��54      $��30��68   $	��180��6D   $��Center=�6P   $��20(�6m   6�   �� 55      $'��!Число ограничений��6   $��30��68   $	��200��6D   $��Center=�6P   $��14(�6t   $��Times New Roman)�6�   �� 56      -   5        $! amountOfRestrictionsTextBox��56      $��30��6C   $	��180��6O   $��Center=�6[   $��20(�6x   6�   �� 57      $��Метод решения��6   $��30��64   $	��200��6@   $��Center=�6L   $��14(�6p   $��Times New Roman)�6~   �� 58      -   5         ��Name$ methodComboBox��58      $	��180��67   $��30��6C   $��16(�6O   .@�59      �� $��Симплекс��6'   �� 5:      $��Графический��6'   5<      �� 5=      $��Вид дробей��6   $��30��61   $	��200��6=   $��Center=�6I   $��14(�6m   $��Times New Roman)�6{   �� 5>      -   5        $ typeOfNumbersComboBox��5>      $	��180��6>   $��30��6J   $��16(�6V   .@�5?      �� $��Обыкновенные��6'   �� 5@      $��Десятичные��6'   5A      �� 5B      $��Тип задачи��6   $��30��61   $	��200��6=   $��Center=�6I   $��14(�6m   $��Times New Roman)�6{   �� 5C      -   5        $ typeOfTaskComboBox��5C      $	��180��6;   $��30��6G   $��16(�6S   .@�5D      �� $"��Минимизировать��6'   �� 5E      $$��Максимизировать��6'   5F      5G      �� 5H      $��1��6   $ Vertical=�6*   $��Center=�6A   $��Center=�6^   $	��200��6y   .+�5I      �� -   5        $ btnRun��5I      $��Center=�6-   $��Center=�6J   $	��150��6e   $��30��6q   $��
Старт��6}   $��20(�6�   6�   �� 5J      -   5        $-��'Случайное заполнение��5J      $��Center=�6<   $��5q�6Y   $��Center=�6d   $��30��6   $	��150��6�   6�   5L      5M      � 5O      $��1��6   .+�5P      � -   5        $ matrixAndButtonAcceptGrid��5P      	 5Q      �� 5R      $��0.99* �6(   �� 5S      $��0.01* �6(   .+�5U      � -   5        $ 
matrixGrid��5U   )   $��0��6   $��1q�6=   
�������6H   5V      5W      5X      5Y      5Z      