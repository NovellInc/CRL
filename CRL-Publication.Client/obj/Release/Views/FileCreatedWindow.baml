   M S B A M L   `   `   `  ����   CRL-Publication.Client;    5Iitrust.CRLPublication.Client.Views.FileCreatedWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � WindowStartupLocation$  CenterScreen=� � WindowStyle$
 None=� � ShowInTaskbar .   � SizeToContent$ WidthAndHeight=� � 
ResizeMode$ NoResize=� � Topmost . .���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $>��8Обнаружен новый файл в системе��$��Center=�
�����_�$
��Bold"�� $��1�� �� ��  � ColumnDefinitions �� $
��Auto ��� .+��� $��0��$��0��$��Имя файла:��$
��Left=�$��2q�$��Right=��� $��0��$��1��$��Right=�$��2q�$��Stretch=����� *Crl.FileName+�� $��1��$��0��$��Имя CRL:��$
��Left=�$��2q�$��Right=��� $��1��$��1��$��Right=�$��2q�$��Stretch=�$��Center=����� *Crl.CrlName+ �� Mode$ TwoWay=��� $��Сохранить��$��2��$��Right=�$��Center=�$��2q����� *SaveSettingsCommand+