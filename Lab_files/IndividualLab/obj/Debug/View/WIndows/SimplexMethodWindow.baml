   M S B A M L   `   `   `  ����H  DIndividualLab, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:IndividualLab.View.WIndowsIndividualLab.View.WIndows  5   	   4    .IndividualLab.View.WIndows.SimplexMethodWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:IndividualLab.View.WIndows     5   	   -   5          � WindowStyle$
  None=�5       � WindowStartupLocation$ CenterScreen=�5   	    � Title$ SimplexMethodWindow��5	   	   $
��1000��6%   $	��830��65    � 
ResizeMode$ 	CanResize=�6v   '  !System.Windows.Shell.WindowChrome   WindowChrome 5
        5   
      GlassFrameThickness$ 0q�6      CornerRadius$ 0��6/      CaptionHeight$ 0Q�6@   .��5      �  � RowDefinitions 5   
   �� 5      $��30 �6   �� 5      $��* �6   .+�5   
   ]� $��0��6   
�������$	��Top=�68   .+�5      �� -   5        $��0��5      $
��Left=�6"   
�������$	��100��6O   $��30��6[   $��
Назад��6g   $��22(�6w   6�   �� 5      $��Right=�6   	 �� Orientation$	 
Horizontal=�60   $��Right=�6I   .+�5      �� -   5        
 ��Name$
 btnMinimize��5      $��-��6.   $��30��6:   $��30��6E   
������ 
������ 5   2   �� 5      -   5        $
 btnMaximize��5      $
��🗖��6.   $��30��6;   $��30��6F   
������ 
������ 5   2   �� 5      -   5        $
 btnClose��5      $��X��6+   $��30��67   $��30��6B   
������ 
������ 5   2   5      5      � 5   
   $��1��6    5      �� 5      $
��0.9* �6    �� 5       $
��0.1* �6    .+�5"      �� 
���   �$��3q�6)   $��0��6=   $��2q�6J   ._�5#      �� -   5         ��Name$ inputScroll��5#      $
��Auto=�64   $
��Auto=�6W   .��5$      �� -   5         ��Name$ outputStackPanel��5$   !   $��1q�6;   $��Stretch=�6F   $	��Top=�6d   5%      5&      5'      �� 5(      $��1��6   
���   �$��3q�66   $��2q�6J   ._�5)      �� $
��Auto=�6   $
��Auto=�6B   .��5*      �� -   5         ��Name$ answerTextBlock��5*       $��24(�69   $
��Left=�6G   $	��Top=�6b   $��2q�5+       $
��Bold"�6+   $��Cascadia Code SemiBold)�6=   $��WrapWithOverflow=�6a   5,      5-      5.      5/      50      