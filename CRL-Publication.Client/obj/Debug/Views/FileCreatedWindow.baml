   M S B A M L   `   `   `  ����  CRL-Publication.Client;    5Iitrust.CRLPublication.Client.Views.FileCreatedWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � WindowStartupLocation$  CenterScreen=�5   	    � WindowStyle$
 None=�5   	    � ShowInTaskbar .   � SizeToContent$ WidthAndHeight=�5   	    � 
ResizeMode$ NoResize=�6(    � Topmost . .��5      �  � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $>��8Обнаружен новый файл в системе��6   $��Center=�69   
�����_�$
��Bold"�6t   � 5   
   $��1��6    5      �� 5      �� 5       � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      .+�5      �� $��0��6   $��0��6!   $��Имя файла:��61   $
��Left=�6F   $��2q�6\   $��Right=�6g   �� 5      $��0��6   $��1��6!   $��Right=�6R   $��2q�6i   $��Stretch=�6t   ��61   �� *Crl.FileName+6t   �� 5      $��1��6   $��0��6!   $��Имя CRL:��61   $
��Left=�6D   $��2q�6Z   $��Right=�6e   �� 5      $��1��6   $��1��6#   $��Right=�63   $��2q�6J   $��Stretch=�6   $��Center=�6�   ��6U   �� *Crl.CrlName+ �� Mode$ TwoWay=�6�   5!      �� 5"   
   $��Сохранить��6   $��2��6%   $��Right=�62   $��Center=�6N   $��2q�6i   ��6t   �� *SaveSettingsCommand+5#      5$      