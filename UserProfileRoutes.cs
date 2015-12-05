using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Foundation.UserProfile
{
    public class UserProfileRoutes : IRouteProvider
    {
        public UserProfileRoutes()
        {}

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {                   
                            new HttpRouteDescriptor {
                                    Name = "profilelogin",
                                    RouteTemplate = "v1/user/login",
                                    Defaults = new { 
                                            action = "Login",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "login"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "facebooklogin",
                                    RouteTemplate = "v1/user/login/facebook",
                                    Defaults = new { 
                                            action = "LoginFacebook",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "login"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "profilelogout",
                                    RouteTemplate = "v1/user/logout",
                                    Defaults = new { 
                                            action = "LogOut",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "login"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "profileregister",
                                    RouteTemplate = "v1/user/register",
                                    Defaults = new { 
                                            action = "Register",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "login"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "rolesbyuser",
                                    RouteTemplate = "v1/user/profile/roles",
                                    Defaults = new { 
                                            action = "Roles",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "profilebyusername",
                                    RouteTemplate = "v1/user/profile('{username}')",
                                    Defaults = new { 
                                            action = "Profile",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "rolesbyprofile",
                                    RouteTemplate = "v1/user/profile('{username}')/roles",
                                    Defaults = new { 
                                            action = "Roles",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "rolebyprofile",
                                    RouteTemplate = "v1/user/profile('{username}')/roles/('{rolename}')",
                                    Defaults = new { 
                                            action = "Roles",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    },
                            new HttpRouteDescriptor {
                                    Name = "profilebyusernamebyfield",
                                    RouteTemplate = "v1/user/profile('{username}')/{field}",
                                    Defaults = new { 
                                            action = "ProfileField",
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    },
                            // default action
                            new HttpRouteDescriptor {
                                    Name = "profiledefault",
                                    RouteTemplate = "v1/user/{action}",
                                    Defaults = new { 
                                            area = "Contrib.Foundation.UserProfile",
                                            controller = "user"
                                        },
                                    }
            };
        }
    }
}