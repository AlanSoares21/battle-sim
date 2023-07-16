import React from 'react';
import { LoginPage } from './pages/LoginPage';
import {
  createBrowserRouter,
  Outlet,
  RouterProvider
} from "react-router-dom";
import { BattlePage } from './pages/BattlePage';
import { AuthContextProvider } from './contexts/AuthContextProvider';
import { CommomDataContextProvider } from './contexts/CommomDataContextProvider';
import { HomePage } from './pages/HomePage';
import EditEntityPage from './pages/EditEntityPage/EditEntityPage';

const Root: React.FC = () => {
  return (
    <AuthContextProvider>
      <CommomDataContextProvider>
        <Outlet />
      </CommomDataContextProvider>
    </AuthContextProvider>
  );
};

const router = createBrowserRouter([
  {
    path: "/",
    element: <Root />,
    children: [
      {
        path: "",
        element: <LoginPage />,
      },
      {
        path: "home",
        element: <HomePage />,
      },
      {
        path: "entity",
        element: <EditEntityPage />,
      },
      {
        path: "battle",
        element: <BattlePage />,
      }
    ]
  },
]);

function Routes() {
  return (<RouterProvider router={router} />);
}

export default Routes;
